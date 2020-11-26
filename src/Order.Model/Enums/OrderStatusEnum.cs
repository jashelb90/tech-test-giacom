using System.ComponentModel;

namespace Order.Model.Enums
{
    public enum OrderStatusEnum
    {
        [Description("Created")]
        created,
        [Description("In Progress")]
        inProgress,
        [Description("Completed")]
        completed,
        [Description("Failed")]
        failed
    }
}