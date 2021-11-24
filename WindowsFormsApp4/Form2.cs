using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiskTest11
{
    public partial class Form2 : Sunny.UI.UIAsideMainFrame
    {
        public Form2()
        {
            InitializeComponent();
            int pageIndex = 100;
            TreeNode parent = Aside.CreateNode("Setting", pageIndex);
            Aside.CreateChildNode(parent, AddPage(new DiskSetting(), ++pageIndex));
            Aside.CreateChildNode(parent, AddPage(new Errors(), ++pageIndex));
            Aside.CreateChildNode(parent, AddPage(new Logging(), ++pageIndex));
            pageIndex = 200;
            parent = Aside.CreateNode("Test", pageIndex);
            Aside.CreateChildNode(parent, AddPage(new Test2(), ++pageIndex));
            Aside.CreateChildNode(parent, AddPage(new Testing(), ++pageIndex));
            Aside.CreateChildNode(parent, AddPage(new Log(), ++pageIndex));
            Aside.CreateChildNode(parent, AddPage(new Information(), ++pageIndex));
            Aside.SelectPage(101);
        }

        private void Header_MenuItemClick(string itemText, int menuIndex, int pageIndex)
        {
            switch (pageIndex)
            {
                default:

                    Aside.SelectPage(pageIndex);
                    break;
            }
        }
    }
}
