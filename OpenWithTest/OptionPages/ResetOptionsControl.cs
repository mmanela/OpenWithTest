using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace MattManela.OpenWithTest.OptionPages
{
    /// <summary>
    /// Custom control for the Reset options page
    /// </summary>
    public partial class ResetOptionsControl : UserControl
    {
        public ResetOptionsControl()
        {
            InitializeComponent();
        }

        public ISolutionIndexService IndexService { get; set; }

        private void deleteSolutionIndexFile_Click(object sender, EventArgs e)
        {
            if (IndexService != null)
            {
                IndexService.DeleteIndexFile();
            }
        }
    }
}
