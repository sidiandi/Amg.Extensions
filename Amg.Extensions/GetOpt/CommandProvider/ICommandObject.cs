namespace Amg.GetOpt
{
    internal interface ICommandObject
    {
        int? OnOptionsParsed(Parser parser);
    }
}