using Amg.FileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace Amg.Collections;

public sealed class PersistentDictionary<K, V> : IDictionary<K, V>, IDisposable
{
    public PersistentDictionary(string file, string? table = null)
    {
        this.table = table ?? $"dic_{typeof(K).Name}_{typeof(V).Name}";

        var connectionStringBuilder = new SQLiteConnectionStringBuilder()
        {
            DataSource = file,
            SyncMode = SynchronizationModes.Off,
            JournalMode = SQLiteJournalModeEnum.Memory
        };
        var create = !file.IsFile();
        connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
        connection.Open();
        if (create)
        {
            Create();
        }
    }

    void Create()
    {
        Execute($"create table {table} ({fKey}, {fValue})");
        Execute($"create unique index index_{table} on {table} ({fKey})");
    }

    readonly string table;
    readonly SQLiteConnection connection;

    public V this[K key]
    {
        get
        {
            return Enumerate($"select {fValue} from {table} where {fKey} = @{fKey}",
                ToParam(fKey, key))
            .Select(_ => FromDbValue<V>(_[0]))
            .First();
        }

        set
        {
            Execute(
                $"insert or replace into {table} values (@key, @value)",
                ToParam(nameof(key), key),
                ToParam(nameof(value), value));
        }
    }

#pragma warning disable S2365 // Properties should not make collection or array copies
    public ICollection<K> Keys => Enumerate($"select key from {table}").Select(_ => FromDbValue<K>(_[0])).ToList();

    public ICollection<V> Values => Enumerate($"select value from {table}").Select(_ => FromDbValue<V>(_[0])).ToList();
#pragma warning restore S2365 // Properties should not make collection or array copies

    public int Count => (int)GetValue<long>($"select count(*) from {table}");

    public bool IsReadOnly => false;

    public void Add(K key, V value)
    {
        Execute(
            $"insert into {table} values (@key, @value)",
            ToParam(nameof(key), key),
            ToParam(nameof(value), value));
    }

    public void Add(KeyValuePair<K, V> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        Execute($"delete from {table}");
    }

    public bool Contains(KeyValuePair<K, V> item)
    {
        return GetValue<long>($"select count(*) from {table} where key = @key and value = @value",
            ToParam(nameof(item.Key), item.Key),
            ToParam(nameof(item.Value), item.Value)
            ) == 1;
    }

    public bool ContainsKey(K key)
    {
        return GetValue<long>($"select count(*) from {table} where key = @key",
            ToParam(nameof(key), key)
            ) == 1;
    }

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
    {
        int i = arrayIndex;
        foreach (var kvp in this)
        {
            array[i++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        var e = Enumerate($"select key, value from {table}")
            .Select(_ => new KeyValuePair<K, V>(FromDbValue<K>(_[0]), FromDbValue<V>(_[1])));

        return e.GetEnumerator();
    }

    public bool Remove(K key)
    {
        return 1 == Execute($"delete from {table} where key = @key", ToParam(fKey, key));
    }

    public bool Remove(KeyValuePair<K, V> item)
    {
        return 1 == Execute($"delete from {table} where {fKey} = @key and {fValue} = @value",
            ToParam(fKey, item.Key),
            ToParam(fValue, item.Value)
            );
    }

    public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
    {
        var v = Enumerate($"select value from {table} where {fKey} = @key",
            ToParam(fKey, key))
            .Select(_ => FromDbValue<V>(_[0]))
            .ToList();

        if (v.Count == 1)
        {
            value = v[0];
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        var e = Enumerate($"select key, value from {table}")
            .Select(_ => new KeyValuePair<K, V>(FromDbValue<K>(_[0]), FromDbValue<V>(_[1])));

        return e.GetEnumerator();
    }

    readonly Dictionary<string, SQLiteCommand> commands = new();

    SQLiteCommand GetCommand(string sql)
    {
        return commands.GetOr(sql, () =>
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            return command;
        });
    }

    int Execute(string sql, params SQLiteParameter[] parameters)
    {
        var command = GetCommand(sql);
        foreach (var p in parameters)
        {
            command.Parameters.Add(p);
        }
        return command.ExecuteNonQuery();
    }

    IEnumerable<object[]> Enumerate(string sql, params SQLiteParameter[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var p in parameters)
        {
            command.Parameters.Add(p);
        }

        using (var reader = command.ExecuteReader())
        {
            var count = reader.FieldCount;
            while (reader.Read())
            {
                var values = new object[count];
                reader.GetValues(values);
                yield return values;
            }
        }
    }

    SQLiteParameter ToParam<T>(string name, T value)
    {
        return new SQLiteParameter(name, ToDbValue(value));
    }

    object ToDbValue<T>(T value)
    {
        string jsonString = JsonSerializer.Serialize(value);
        return jsonString;
    }

    T FromDbValue<T>(object dbValue)
    {
        var jsonString = dbValue as string;
        if (jsonString is null) throw new ArgumentOutOfRangeException(nameof(dbValue), dbValue, "Must be a JSON string");
        var r = JsonSerializer.Deserialize<T>(jsonString);
        return r!;
    }

    T GetValue<T>(string sql, params SQLiteParameter[] parameters)
    {
        return Enumerate(sql, parameters).Select(_ => (T)_[0]).First();
    }

    public void Dispose()
    {
        connection.Close();
    }

    const string fValue = "value";
    const string fKey = "key";

}


