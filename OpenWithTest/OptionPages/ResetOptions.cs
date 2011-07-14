using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace MattManela.OpenWithTest.OptionPages
{
    /// <summary>
    /// Reset options page
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("FF6CDA3D-C1DE-47E3-BDC8-8964EADD91E5")]
    public class ResetOptions : DialogPage
    {
        public ResetOptions()
        {
          
        }

        private ResetOptionsControl control;

        public ISolutionIndexService IndexService { get; set; }


        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                control = new ResetOptionsControl();
                control.IndexService = IndexService;
                control.Location = new Point(0, 0);
                return control;
            }
        }
    }
}

