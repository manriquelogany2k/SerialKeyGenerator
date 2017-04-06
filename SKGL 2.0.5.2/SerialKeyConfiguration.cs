namespace SerialKeyGenerator
{
    public class SerialKeyConfiguration : BaseConfiguration
    {


        private bool[] _features = new bool[8] {
            //the default value of the Features array.
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };


        public virtual bool[] Features
        {
            get => _features;
            set => _features = value;
        }
        private bool _addSplitChar = true;


        public bool AddSplitChar
        {
            get => _addSplitChar;
            set => _addSplitChar = value;
        }



    }
}