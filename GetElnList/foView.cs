using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetElnList
{
    public partial class foView : Form
    {
        public string xmlFileName { get; set; }
        public foView()
        {
            InitializeComponent();
        }

        private void foView_Shown(object sender, EventArgs e)
        {

            webBrowser1.Navigate(xmlFileName);
        }
    }
}
