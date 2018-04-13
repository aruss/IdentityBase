namespace IdentityBase.ModelState
{
    using System.Collections.Generic;

    public class ModelStateTransferValue
    {
        public ModelStateTransferValue()
        {
            this.ErrorMessages = new List<string>();
        }

        public string Key { get; set; }
        public string AttemptedValue { get; set; }
        public object RawValue { get; set; }
        public ICollection<string> ErrorMessages { get; set; }
    }
}
