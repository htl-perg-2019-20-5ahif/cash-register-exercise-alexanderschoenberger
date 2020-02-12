using CashRegister.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CashRegister.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products
        {
            get { return products; }
            set { products = value; OnPropertyChanged(nameof(Products)); }
        }

        private ObservableCollection<Product> products;

        public ObservableCollection<ReceiptLine> Basket
        {
            get { return basket; }
            set { basket = value; OnPropertyChanged(nameof(Basket)); }
        }

        private ObservableCollection<ReceiptLine> basket = new ObservableCollection<ReceiptLine>();
        private HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/api/")
        };
        private decimal totalSum;
        public decimal TotalSum { get { return totalSum; } set { totalSum = value; OnPropertyChanged(nameof(TotalSum)); } }
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Loaded += async (_, __) => await InitAsync();
        }

        public async Task InitAsync()
        {
            string x = await httpClient.GetStringAsync("products");
            Products = JsonSerializer.Deserialize<ObservableCollection<Product>>(x);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private void AddToBasket(object sender, RoutedEventArgs e)
        {
            string s = (sender as Button).Content.ToString();
            Product product = Products.First((Product p) => p.ProductName.Equals(s));
            if (!Basket.Any(b => b.Product.ID == product.ID))
            {
                Basket.Add(new ReceiptLine { Amount = 1, Product = product, TotalPrice = product.UnitPrice });
            }
            else
            {
                ReceiptLine receiptLine = Basket.First(p => p.Product.ID == product.ID);
                receiptLine.Amount++;
                receiptLine.TotalPrice += product.UnitPrice;
                OnPropertyChanged(nameof(Basket));
            }
            TotalSum = Basket.Sum(p => p.TotalPrice);
        }

        private async void OnCheckout(object sender, RoutedEventArgs e)
        {
            // Turn all items in the basket into DTO objects
            var dto = Basket.Select(b => new ReceiptLineDto
            {
                ProductID = b.Product.ID,
                Amount = b.Amount
            }).ToList();

            // Create JSON content that can be sent using HTTP POST
            using (var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"))
            {
                // Send the receipt to the backend
                var response = await httpClient.PostAsync("/api/receipts", content);
                response.EnsureSuccessStatusCode();
            }

            // Clear basket so shopping can start from scratch
            Basket.Clear();
            TotalSum = 0;
        }
    }
}
