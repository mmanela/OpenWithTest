using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace MattManela.OpenWithTest.OptionPages
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("5A980FEA-05F5-4ABB-B022-224C4B20F2AC")]
    public class OpenWithTestSettings : DialogPage, INotifyPropertyChanged
    {
        public OpenWithTestSettings()
        {
            EnableAutoOpen = true;
            TestClassSuffixes = new List<string>
                                    {
                                        "facts",
                                        "fact",
                                        "test",
                                        "tests"
                                    };
        }

        private bool enableAutoOpen;

        [DisplayName("Enable Auto Open")]
        [Description("Open test file when you open the implementation file and vice versa")]
        public bool EnableAutoOpen
        {
            get { return enableAutoOpen; }
            set { 
                enableAutoOpen = value;
                OnPropertyChange("EnableAutoOpen");
            }
        }


        [DisplayName("Test Class Suffixes")]
        [Description("The suffixes which determine test class files.")]
        [EditorAttribute(typeof(StringCollectionEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(StringCollectionConvertor))]
        public List<string> TestClassSuffixes
        {
            get; set;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChange(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}