namespace MedicineInventory.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string DOJ { get; set; }
        public string Token { get; set; }
    }
}
