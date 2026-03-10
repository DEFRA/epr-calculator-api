namespace EPR.Calculator.API.Data.Enums
{
    public enum RagRating
    {
        Red,
        Amber,
        Green,
        RedMedical,
        AmberMedical,
        GreenMedical
    }

    public static class RagRatingExtensions
    {
        public static RagRating ParseRag(string value) =>
            value switch
            {
                "R" => RagRating.Red,
                "A" => RagRating.Amber,
                "G" => RagRating.Green,
                "R-M" => RagRating.RedMedical,
                "A-M" => RagRating.AmberMedical,
                "G-M" => RagRating.GreenMedical,
                _ => throw new ArgumentException($"Invalid RAG value '{value}'")
            };

        public static string ToDbValue(this RagRating rag) =>
            rag switch
            {
                RagRating.Red => "R",
                RagRating.Amber => "A",
                RagRating.Green => "G",
                RagRating.RedMedical => "R-M",
                RagRating.AmberMedical => "A-M",
                RagRating.GreenMedical => "G-M",
                _ => throw new ArgumentException($"Invalid RAG value '{rag}'")
            };
    }
}