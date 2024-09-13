using System;
using System.Collections.Generic;
using System.Linq;

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

            warehouse.Delive(iPhone12, 10);
            warehouse.Delive(iPhone11, 1);

            //Вывод всех товаров на складе с их остатком

            Cart cart = shop.Cart();
            cart.Delive(iPhone12, 4);
            cart.Delive(iPhone11, 1); //при такой ситуации возникает ошибка так, как нет нужного количества товара на складе

            //Вывод всех товаров в корзине

            Console.WriteLine(cart.Order().Paylink);

            cart.Delive(iPhone12, 9); //Ошибка, после заказа со склада убираются заказанные товары
        }
    }

    public class Cart : ProductManager
    {
        private IWarehouse _warehouse;
        private List<IReadOnlyCell> _order;

        public Cart(IWarehouse warehouse)
        {
            _warehouse = warehouse;

            _order = new List<IReadOnlyCell>();
        }

        public override void Delive(Good product, int count)
        {
            IReadOnlyCell warehouseCell = GetCell(_warehouse.Cells, product);

            if (warehouseCell == null)
                throw new ArgumentNullException(nameof(warehouseCell));

            if (count > warehouseCell.Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            PutIn(product, count);
        }

        public override void Remove(Good product, int count) =>
            throw new NotImplementedException();

        public Order Order()
        {
            string order = $"Ваша корзина\n";

            foreach (Cell cell in _order)
            {
                _warehouse.Remove(cell.Product, cell.Count);

                order += $"{cell.Product.Name}: {cell.Count}\n";
            }

            return new Order(order);
        }

        private void PutIn(Good product, int count)
        {
            IReadOnlyCell cartCell = GetCell(_order, product);

            if (cartCell != null)
            {
                _order.Insert(_order.IndexOf(cartCell), new Cell(product, cartCell.Count + count));
                _order.RemoveAt(_order.IndexOf(cartCell));
                return;
            }

            _order.Add(new Cell(product, count));
        }
    }

    public class Shop
    {
        public Shop(Warehouse warehouse) =>
            Warehouse = warehouse ?? throw new ArgumentNullException(nameof(warehouse));

        public Warehouse Warehouse { get; private set; }

        public Cart Cart() =>
            new Cart(Warehouse);
    }

    public class Warehouse : ProductManager, IWarehouse
    {
        private readonly List<IReadOnlyCell> _cells;

        public Warehouse() =>
            _cells = new List<IReadOnlyCell>();

        public IReadOnlyList<IReadOnlyCell> Cells =>
            _cells;

        public override void Delive(Good product, int count) =>
            _cells.Add(new Cell(product, count));

        public override void Remove(Good product, int count)
        {
            IReadOnlyCell reqieredCell = GetCell(_cells, product);

            _cells.Insert(_cells.IndexOf(reqieredCell), new Cell(product, reqieredCell.Count - count));
        }
    }

    public class Cell : IReadOnlyCell
    {
        public Cell(Good product, int count)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Count = count >= 0 ? count : throw new ArgumentOutOfRangeException(nameof(count));
        }

        public Good Product { get; private set; }
        public int Count { get; private set; }
    }

    public class Good
    {
        public Good(string name) =>
            Name = name;

        public string Name { get; }
    }

    public class Order
    {
        public Order(string paylink) =>
            Paylink = paylink;

        public string Paylink { get; }
    }

    public abstract class ProductManager
    {
        public abstract void Delive(Good product, int count);

        public abstract void Remove(Good product, int count);

        public IReadOnlyCell GetCell(IReadOnlyList<IReadOnlyCell> cells, Good product) =>
            cells.FirstOrDefault(cell => cell.Product == product);
    }

    public interface IReadOnlyCell
    {
        Good Product { get; }
        int Count { get; }
    }

    public interface IWarehouse
    {
        IReadOnlyList<IReadOnlyCell> Cells { get; }

        void Remove(Good product, int count);
    }
}
