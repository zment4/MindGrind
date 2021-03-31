using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasterThrind
{
    public partial class Form1 : Form
    {
        private List<ListView> resultListViews;
        private List<ListView> previousListViews = new List<ListView>();
        private ListView currentListView;
        private List<string> correctLine = new List<string>();

        public Form1()
        {
            InitializeComponent();

            currentListView = listViewItem1;
            resultListViews = new List<ListView>() { listViewResult1, listViewResult2, listViewResult3 };

            var random = new Random();
            var possibleSymbols = new string[] { "A1", "A2", "A3", "B1", "B2", "B3", "C1", "C2", "C3" }.ToList();
            for (int i = 0; i < 3; i++)
            {
                var symbolIndex = random.Next(0, possibleSymbols.Count());
                correctLine.Add(possibleSymbols[symbolIndex]);
                possibleSymbols.RemoveAt(symbolIndex);
            }
        }

        private void symbol1_Click(object sender, EventArgs e)
        {
            addSymbol("A1");
        }

        private void symbol2_Click(object sender, EventArgs e)
        {
            addSymbol("A2");
        }

        private void symbol3_Click(object sender, EventArgs e)
        {
            addSymbol("A3");
        }

        private void symbol4_Click(object sender, EventArgs e)
        {
            addSymbol("B1");
        }

        private void symbol5_Click(object sender, EventArgs e)
        {
            addSymbol("B2");
        }

        private void symbol6_Click(object sender, EventArgs e)
        {
            addSymbol("B3");
        }

        private void symbol7_Click(object sender, EventArgs e)
        {
            addSymbol("C1");
        }

        private void symbol8_Click(object sender, EventArgs e)
        {
            addSymbol("C2");
        }

        private void symbol9_Click(object sender, EventArgs e)
        {
            addSymbol("C3");
        }

        private void addSymbol(string symbol)
        {
            if (previousListViews.Select(x => x.Items.Cast<ListViewItem>().Last()).Any(x => x.Text.Equals(symbol)))
            {
                return;
            }

            currentListView.Items.Add(symbol);

            previousListViews.Add(currentListView);

            if (currentListView == listViewItem1)
            {
                currentListView = listViewItem2;
                return;
            }

            if (currentListView == listViewItem2)
            {
                currentListView = listViewItem3;
                return;
            }

            // It was not item1 or item2 so it must be item3
            checkLine();

            previousListViews.Clear();
            currentListView = listViewItem1;
        }

        private void checkLine()
        {
            var currentLine = (new ListView[] { listViewItem1, listViewItem2, listViewItem3 }).Select(
                x => x.Items.Cast<ListViewItem>().Last().Text
                ).ToList();

            var currentResultListViewIndex = 0;

            var correctSymbolsToHandle = correctLine.ToList();
            var currentSymbolsToHandle = currentLine.ToList();

            var correctLineSelection = correctLine.Select((value, i) => new { i, value });
            // Check for full on correct positions
            foreach (var item in correctLineSelection)
            {
                if (item.value == currentLine[item.i])
                {
                    resultListViews[currentResultListViewIndex].Items.Add("X");
                    currentResultListViewIndex++;
                    correctSymbolsToHandle.Remove(item.value);
                    currentSymbolsToHandle.Remove(item.value);
                }
            }

            // Check for correct symbol but wrong position
            foreach (var item in correctLineSelection.Where(x => correctSymbolsToHandle.Any(y => y == x.value)))
            {
                if (currentLine.ToList().Where(x => currentSymbolsToHandle.Any(y => y == x)).Any(x => x == item.value))
                {
                    resultListViews[currentResultListViewIndex].Items.Add("O");
                    currentResultListViewIndex++;
                    correctSymbolsToHandle.Remove(item.value);
                    currentSymbolsToHandle.Remove(item.value);
                }
            }

            // Check for correct character 
            foreach (var item in correctLineSelection.Where(x => correctSymbolsToHandle.Any(y => y == x.value)))
            {
                if (currentLine.ToList().Where(x => currentSymbolsToHandle.Any(y => y == x)).Any(x => x[0] == item.value[0]))
                {
                    resultListViews[currentResultListViewIndex].Items.Add("C");
                    currentResultListViewIndex++;
                    correctSymbolsToHandle.Remove(item.value);
                    currentSymbolsToHandle.Remove(item.value);
                }
            }

            // Check for correct number
            foreach (var item in correctLineSelection.Where(x => correctSymbolsToHandle.Any(y => y == x.value)))
            {
                if (currentLine.ToList().Where(x => currentSymbolsToHandle.Any(y => y == x)).Any(x => x[1] == item.value[1]))
                {
                    resultListViews[currentResultListViewIndex].Items.Add("N");
                    currentResultListViewIndex++;
                    correctSymbolsToHandle.Remove(item.value);
                    currentSymbolsToHandle.Remove(item.value);
                }
            }

            for (; currentResultListViewIndex < 3; currentResultListViewIndex++)
            {
                resultListViews[currentResultListViewIndex].Items.Add(" ");
            }
        }
    }
}
