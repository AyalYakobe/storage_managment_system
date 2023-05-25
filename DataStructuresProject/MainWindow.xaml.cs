using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Models_DS;
using BL_DS;
using BL_DS.InnerData;

namespace DataStructuresProject
{
    public partial class MainWindow : Window, ICommunication
    {
        Manager _manager;
        bool _wantsSplit;
        bool _isPartialPurchase;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                //Loads items from the text file into the trees
                _manager = Manager.OnInit(this);
                if (_manager == null)
                {
                    _manager = new Manager(10, 2, new TimeSpan(0, 20, 10), new TimeSpan(0, 20, 10), this);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("An unknown error ocured.");
            }

        }

        private void btnDeposit_Click(object sender, RoutedEventArgs e) //Deposits a box into the storage program, if a maximum is reached, a message will warn the user 
        {
            try
            {
                if (_manager.AddBoxes(double.Parse(txtXDimensions.Text), double.Parse(txtYDimensions.Text), int.Parse(txtQuantity.Text)))
                {
                    MessageBox.Show($"Deposit Succeeded", "Box Store", MessageBoxButton.OK);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("The maximum quantity per item is 10.", "Box Store", MessageBoxButton.OK);
                }
                ResetText();
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid input");
            }
            catch (Exception)
            {
                MessageBox.Show("An unknown error ocured.");
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e) //Searches for a specific box in the program, if it is not found, a message will be sent to the user
        {
            try
            {
                listBox.Items.Add(_manager.ExactInfo(double.Parse(txtXDimensions.Text), double.Parse(txtYDimensions.Text)));
                ResetText();
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid input");
            }
            catch (Exception)
            {
                MessageBox.Show("An unknown error ocured.");
            }
        }
        private void btnPurchase_Click(object sender, RoutedEventArgs e) //Allows the user to purchase items 
        {
            try
            {
                if (_manager.Buy(double.Parse(txtXDimensions.Text), double.Parse(txtYDimensions.Text), int.Parse(txtQuantity.Text)))
                {
                    MessageBox.Show("Purchase Succeeded", "Box Store", MessageBoxButton.OK);
                }
                else
                {
                    if (_wantsSplit)
                    {
                        MessageBox.Show("Purchase Succeeded", "Box Store", MessageBoxButton.OK);
                    }
                    else if (_isPartialPurchase)
                    {
                        MessageBox.Show("Due to a lack of items in stock, only a portion of your purchase was executed.");
                        _isPartialPurchase = false;
                    }
                    else
                    {
                        MessageBox.Show("Purchase Failed", "Box Store", MessageBoxButton.OK);
                    }               
                }
                ResetText();
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid input");
            }
            catch (Exception)
            {
                MessageBox.Show("An unknown error occured.");
            }
        }
        private void btnClear_Click(object sender, RoutedEventArgs e) //Clears items from the storage feed 
        {
            ResetText();
            listBox.Items.Clear();
        }

        void ICommunication.ReturnLongReceipt (List<Box> splitsList) //Returns a long list of purchased items 
        {
            listBox.Items.Add("RECIEPT");
            foreach (var item in splitsList)
            {
                listBox.Items.Add($"\nX: {item._X._Data}\nY: {item._Y._Data}\n" +
                        $"Amount Purchased: {item._Y._Count}\nPurchased Date: {item._LastPurchase}\nPURCHASED\n");          
            }
        }
        void ICommunication.ReturnShortReceipt(List<Box> splitsList, int amount) //Returns one purchased item 
        {
            listBox.Items.Add("RECIEPT");
            foreach (var item in splitsList)
            {
                listBox.Items.Add($"\nX: {item._X._Data}\nY: {item._Y._Data}\n" +
                    $"Amount Purchased: {amount}\nPurchased Date: {item._LastPurchase}\nPURCHASED\n");
            }
        }
        void ICommunication.ReorderAlert(Box question) //Alerts the user when a reorder is necessary 
        {
            MessageBox.Show($"The item you have purchased is runnning low or out of stock.\n" +
                $"(X: { question._X._Data}, Y: { question._Y._Data})");
        }
        bool ICommunication.WhetherToContinue(Box question, DataX dataX, DataY dataY) //Asks the user if he would like to continue 
        {
            if (question._X._Data * question._Y._Data * 1.5 > dataX._Data * dataY._Data)
            {
                MessageBoxResult res1 = MessageBox.Show($"DISCLAIMER: The closest item in size" +
                    $" is at least 50 percent larger than your chosen item. Would you like us to substitute your order with next smallest size?" +
                $"\nX: { question._X._Data}\nY: { question._Y._Data}\n{question._LastPurchase.ToShortDateString()}", "Box Store", MessageBoxButton.YesNo);

                if (res1 == MessageBoxResult.Yes)
                {
                    _wantsSplit = true;
                    return true;
                }
                else
                {
                    _wantsSplit = false;
                    return false;
                }
            }
            else
            {
                MessageBoxResult res2 = MessageBox.Show($"DISCLAIMER: The box you have chosen has not been found in storage. " +
                    $"\nX: { question._X._Data}\nY: { question._Y._Data}\n{question._LastPurchase.ToShortDateString()}");
                
                if (res2 == MessageBoxResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        void ICommunication.PurchaseInfo(DataX x, DataY y) //Gives the details of a purchased item 
        {
            listBox.Items.Add($"X: {x._Data}\nY: {y._Data}\nBoxes In Storage: {y._Count}\n");
        }
        void ICommunication.ExpiredUpdate(DataX x, DataY y) //Updates the user if an item has been removed 
        {
            MessageBox.Show($"The follwoing box was in storage for too long and was automatically removed:\n" +
                $"X: {x._Data}, Y: {y._Data}");
        }
        void ICommunication.PartialPurchase(bool isPartiallyPurchased)
        {
            if (isPartiallyPurchased)
            {
                _isPartialPurchase = true;
            }
            else
            {
                _isPartialPurchase = false;
            }
        }

        private void ResetText()
        {
            foreach (var item in grid.Children.OfType<TextBox>())
            {
                item.Text = "";
            }
        }

        private void On_Closing(object sender, System.ComponentModel.CancelEventArgs e) //Calls the OnExit function in the manager class when the program is closing  
        {
            try
            {
                _manager.OnExit();
            }
            catch (Exception)
            {
                MessageBox.Show("An unknown error ocured.");
            }
        }
    }
}
