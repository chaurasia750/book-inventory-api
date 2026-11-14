namespace BookInventory.BuildingBlocks.Configuration.Filters
{
    /// <summary>
    /// Marks an action (or controller) whose database changes must be committed even when it answers with a 4xx/5xx
    /// <em>result</em> (for example a failed login that has to record the attempt and increment the lockout counter).
    /// Without it the request transaction rolls back on any error status. An unhandled exception still rolls back.
    /// Use sparingly and only where a failure must leave a trace: return a result, do not throw.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class PersistOnFailureAttribute : Attribute;
}
