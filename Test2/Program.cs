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

    public class Cart
    {
        private readonly IWarehouse _warehouse;
        private readonly List<Cell> _order;

        public Cart(IWarehouse warehouse)
        {
            _warehouse = warehouse;

            _order = new List<Cell>();
        }

        public void Delive(Good product, int count)
        {
            Cell warehouseCell = _warehouse.GetCell(_warehouse.Cells, product);

            if (warehouseCell == null)
                throw new ArgumentNullException(nameof(warehouseCell));

            if (_order.Count > 0)
            {
                Cell cell = _warehouse.GetCell(_order, product);

                if (_order.Contains(cell))
                    if (_order[_order.IndexOf(cell)].Count + count > warehouseCell.Count)
                        throw new ArgumentOutOfRangeException(nameof(cell.Count));
            }
            else if (count > warehouseCell.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            PutIn(product, count);
        }

        public Order Order()
        {
            string order = $"Ваша корзина\n";

            foreach (Cell cell in _order)
            {
                _warehouse.Remove(cell.Product, cell.Count);

                order += $"{cell.Product.Name}: {cell.Count}\n";
            }

            _order.Clear();

            return new Order(order);
        }

        private void PutIn(Good product, int count)
        {
            Cell cartCell = _warehouse.GetCell(_order, product);

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

        public Warehouse Warehouse { get; }

        public Cart Cart() =>
            new Cart(Warehouse);
    }

    public class Warehouse : IWarehouse
    {
        private readonly List<Cell> _cells;

        public Warehouse() =>
            _cells = new List<Cell>();

        public IReadOnlyList<Cell> Cells =>
            _cells;

        public void Delive(Good product, int count) =>
            _cells.Add(new Cell(product, count));

        public void Remove(Good product, int count)
        {
            Cell reqieredCell = GetCell(_cells, product);

            _cells.Insert(_cells.IndexOf(reqieredCell), new Cell(product, reqieredCell.Count - count));
        }

        public Cell GetCell(IReadOnlyList<Cell> cells, Good product) =>
            cells.FirstOrDefault(cell => cell.Product == product);
    }

    public class Cell
    {
        public Cell(Good product, int count)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Count = count >= 0 ? count : throw new ArgumentOutOfRangeException(nameof(count));
        }

        public Good Product { get; }
        public int Count { get; }
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

    public interface IWarehouse
    {
        IReadOnlyList<Cell> Cells { get; }

        void Remove(Good product, int count);

        void Delive(Good product, int count);

        Cell GetCell(IReadOnlyList<Cell> cells, Good product);
    }
}
