using System;
using System.Collections.Generic;

namespace Test2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Good iPhone12 = new Good("IPhone 12");
            Good iPhone11 = new Good("IPhone 11");

            Warehouse warehouse = new Warehouse();

            Shop shop = new Shop(warehouse);

            warehouse.Delive(iPhone12, 100);
            warehouse.Delive(iPhone11, 100);

            //Вывод всех товаров на складе с их остатком

            Cart cart = shop.Cart();
            cart.Add(iPhone12, 4);
            cart.Add(iPhone11, 3); //при такой ситуации возникает ошибка так, как нет нужного количества товара на складе

            //Вывод всех товаров в корзине

            Console.WriteLine(cart.Order().Paylink);

            cart.Add(iPhone12, 9); //Ошибка, после заказа со склада убираются заказанные товары
        }
    }

    public class Cart
    {
        private readonly IWarehouse _warehouse;
        private readonly IDictionary<Good, int> _order = new Dictionary<Good, int>();

        public Cart(IWarehouse warehouse) =>
            _warehouse = warehouse ?? throw new ArgumentNullException($"'{nameof(warehouse)}' is null");

        public void Add(Good product, int count)
        {
            if (product == null)
                throw new ArgumentNullException($"'{nameof(product)}' is null");

            if (count <= 0)
                throw new ArgumentOutOfRangeException($"'{nameof(count)}' is less than or equal to zero");

            if (!_warehouse.Contains(product, count))
                throw new ArgumentException($"'{product.Name}' doesn't exist in count of {count}");

            if (!_order.ContainsKey(product))
                _order[product] = 0;

            _order[product] += count;
        }

        public Order Order()
        {
            string order = $"Ваша корзина:\n\n";

            foreach (Good product in _order.Keys)
            {
                _warehouse.Remove(product, _order[product]); 

                order += $"Товар: {product.Name}\nКол-во: {_order[product]}\n\n";
            }

            _order.Clear();

            return new Order(order);
        }
    }

    public class Order
    {
        public Order(string paylink) =>
            Paylink = paylink;

        public string Paylink { get; }
    }

    public class Shop
    {
        private readonly Warehouse _warehouse;

        public Shop(Warehouse warehouse) =>
            _warehouse = warehouse ?? throw new ArgumentNullException($"'{nameof(warehouse)}' is null");

        public Cart Cart() =>
            new Cart(_warehouse);
    }

    public interface IWarehouse
    {
        void Remove(Good product, int count);

        bool Contains(Good product, int count);
    }

    public class Warehouse : IWarehouse
    {
        private readonly IDictionary<Good, int> _goods = new Dictionary<Good, int>();

        public void Delive(Good product, int count)
        {
            ExceptionCheck(product, count);

            if (!_goods.ContainsKey(product))
                _goods[product] = 0;

            _goods[product] += count;
        }

        public void Remove(Good product, int count)
        {
            ExceptionCheck(product, count);

            if (!Contains(product, count))
                throw new InvalidOperationException($"'{product.Name}' doesn't exist in count of {count}");

            _goods[product] -= count;
        }

        public bool Contains(Good product, int count)
        {
            if (!_goods.TryGetValue(product, out int value))
                return false;                   

            if (count > value) 
                return false;

            return true;
        }

        private void ExceptionCheck(Good product, int count)
        {
            if (product == null)
                throw new ArgumentNullException($"'{nameof(product)}' is null");

            if (count <= 0)
                throw new ArgumentOutOfRangeException($"'{nameof(count)}' is less than or equal to zero");
        }
    }

    public class Good
    {
        public Good(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException($"'{nameof(name)}' string is empty");

            Name = name;
        }

        public string Name { get; }
    }
}
