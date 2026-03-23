namespace ProRental.Domain.Entities
{
    public class Catalog
    {
        public int Id { get; set; }

        // existing fields (already in your DB)
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        // Feature 5 fields (ONLY if already part of schema/team agrees)
        public string EcoBadge { get; set; }
        public decimal CarbonScore { get; set; }
    }
}   