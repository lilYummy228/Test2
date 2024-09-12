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
            cart.Add(iPhone12, 4);
            cart.Add(iPhone11, 1); //при такой ситуации возникает ошибка так, как нет нужного количества товара на складе

            //Вывод всех товаров в корзине

            Console.WriteLine(cart.Order().Paylink);

            cart.Add(iPhone12, 9); //Ошибка, после заказа со склада убираются заказанные товары
        }
    }

    public class Cart
    {
        private IReadOnlyList<IReadOnlyCell> _goods;  
        private IWarehouse _warehouse;
        private List<Cell> _order;

        public Cart(IWarehouse warehouse)
        {
            _warehouse = warehouse;
            _goods = _warehouse.Cells;

            _order = new List<Cell>();
        }

        public void Add(Good product, int count)
        {
            IReadOnlyCell cell = _goods.FirstOrDefault(cells => cells.Product == product);

            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            if (count > cell.Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            PutIn(product, count);
        }

        public void PutIn(Good product, int count)
        {
            Cell cartCell = FindCell(_order, product);

            if (cartCell == null)
            {
                _order.Insert(_order.IndexOf(cartCell), new Cell(product, cartCell.Count + count));
                _order.RemoveAt(_order.IndexOf(cartCell));
                return;
            }

            _order.Add(new Cell(product, count));
        }

        public Order Order()
        {
            string order = $"Ваша корзина\n";

            foreach (Cell cell in _order)
            {
                _warehouse.Remove(FindCell((List<Cell>)_warehouse.Cells, cell.Product), cell.Count);

                order += $"{cell.Product.Name}: {cell.Count}\n";
            }

            return new Order(order);
        }

        private Cell FindCell(List<Cell> cells, Good product) => 
            cells.FirstOrDefault(cell => cell.Product == product);
    }

    public class Shop
    {
        public Warehouse Warehouse { get; private set; }

        public Shop(Warehouse warehouse) =>
            Warehouse = warehouse ?? throw new ArgumentNullException(nameof(warehouse));

        public Cart Cart() =>
            new Cart(Warehouse);
    }

    public class Warehouse : IWarehouse
    {
        private readonly List<Cell> _cells;

        public Warehouse() =>
            _cells = new List<Cell>();

        public IReadOnlyList<IReadOnlyCell> Cells =>
            _cells;

        public void Delive(Good good, int count) =>
            _cells.Add(new Cell(good, count));

        public void Remove(Cell cell, int count) =>
            _cells.Insert(_cells.IndexOf(cell), new Cell(cell.Product, cell.Count - count));
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

    public interface IReadOnlyCell
    {
        Good Product { get; }
        int Count { get; }
    }

    public interface IWarehouse
    {
        IReadOnlyList<IReadOnlyCell> Cells { get; }

        void Remove(Cell cell, int count);
    }
}
