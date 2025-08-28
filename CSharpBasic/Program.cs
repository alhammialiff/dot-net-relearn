namespace CSharpBasic
{

    // Interface for discountable products
    public interface IDiscountable
    {

        decimal ApplyDiscount(decimal percentage);

    }

    // Base Product class with fields, properties, and static methods
    class Product
    {

        // Private field
        private decimal _price;

        // Public property
        public string Name { get; set; }

        // Public property with additional logic in setter
        public decimal Price
        {

            get { return _price; }
            set
            {
                if (value >= 0)
                {
                    _price = value;
                }
            }

        }

        // Constructor
        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }


        // Virtual method can be overridden by any class that inherits Product
        // So if there is an Electronic Product that inherits from Product
        // DisplayProductDetails can be overridden via 
        public virtual void DisplayProductDetails()
        {
            Console.WriteLine($"Product: {Name}, Price: {Price:C}");
        }

        public static decimal CalculateDiscount(decimal price, decimal discountPercentage)
        {
            return price - (price * discountPercentage / 100);
        }


    }

    class Clothing : Product, IDiscountable
    {

        // Property to store the size as an integer
        public int Size { get; set; }

        public Clothing(string name, decimal price, int size) : base(name, price)
        {
            Size = size;
        }

        public string GetSizeName()
        {

            return Size switch
            {
                1 => "SM",
                2 => "MD",
                3 => "LG",
                _ => "Unknown Size"
            };

        }

        public override void DisplayProductDetails()
        {
            base.DisplayProductDetails();
            Console.WriteLine($"Size: {GetSizeName()}");
        }

        // IDiscountable enforces that we include ApplyDiscount(decimal percentage)
        public decimal ApplyDiscount(decimal percentage)
        {
            return CalculateDiscount(Price, percentage);
        }

    }

    class Program
    {

        static void Main()
        {
            List<Clothing> catalog = new List<Clothing>();

            // Create clothing objects and add into catalog
            catalog.Add(new Clothing("Super Cool T-shirt", 49.99m, 2));
            catalog.Add(new Clothing("Short Pants", 79.99m, 1));
            catalog.Add(new Clothing("Short Pants", 82.99m, 2));

            for (int i = 0; i < catalog.Count; i++)
            {

                catalog[i].DisplayProductDetails();

            }

            foreach (Clothing item in catalog)
            {
                item.DisplayProductDetails();
            }

            decimal discountedPrice = catalog[0].ApplyDiscount(10);

            // :C formats discountedPrice as 'currency' value
            Console.WriteLine($"T-shirt price after discount: {discountedPrice:C}");

            // the 'm' tells C# to treat this number as decimal instead of double
            // decimal type is more precise for financial and monetary calculations
            Console.WriteLine(Product.CalculateDiscount(29.50m, 0.1m));

        }

    }

}