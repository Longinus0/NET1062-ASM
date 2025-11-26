namespace ASM.Models
{
    public class Combo
    {
        public int ComboId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public long Price { get; set; }
        public string? Image { get; set; }
        public bool IsAvailable { get; set; } = true;

        public ICollection<ComboDetail> ComboDetails { get; set; } = new List<ComboDetail>();
    }
}
