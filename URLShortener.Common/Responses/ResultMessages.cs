namespace URLShortener.Common.Responses;

public static class ResultMessages
{
    public static Dictionary<OperationOutcomes, string> LiteralMessages = new()
    {
        { OperationOutcomes.Success, "Operation Successful." },
        { OperationOutcomes.NotFound, "Requested Resource Was Not Found." },
        { OperationOutcomes.InternalError, "Internal Error Processing Your Request." },
        { OperationOutcomes.InsufficientBalance, "User Does Not Have Enough Balance To Perform That Action." }
    };

    public static Dictionary<OperationOutcomesFormatted, string> FormattedMessages = new()
    {
        { OperationOutcomesFormatted.NotFound, "{0} For {1} Was Not Found." }
    };
}

public enum OperationOutcomes
{
    Success,
    NotFound,
    InsufficientBalance,
    InternalError
}

public enum OperationOutcomesFormatted
{
    NotFound,
}