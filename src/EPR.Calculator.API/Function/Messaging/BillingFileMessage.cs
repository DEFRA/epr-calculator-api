namespace EPR.Calculator.Service.Function.Messaging
{
    /// <summary>
    /// Billing file message model class.
    /// </summary>
    public record BillingFileMessage
    {
        /// <summary>
        /// Gets or sets the identifier for the calculator run.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user who approved the billing file.
        /// </summary>
        required public string ApprovedBy { get; set; }

        /// <summary>
        /// Gets or sets the message type for calculator.
        /// </summary>
        required public string MessageType { get; set; }
    }
}
