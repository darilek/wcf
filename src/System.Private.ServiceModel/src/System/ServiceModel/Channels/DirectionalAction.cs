using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
    internal class DirectionalAction : IComparable<DirectionalAction>
    {
        private MessageDirection direction;
        private string action;
        private bool isNullAction;

        internal DirectionalAction(MessageDirection direction, string action)
        {
            if (!MessageDirectionHelper.IsDefined(direction))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentOutOfRangeException(nameof(direction)));
            this.direction = direction;
            if (action == null)
            {
                this.action = "*";
                this.isNullAction = true;
            }
            else
            {
                this.action = action;
                this.isNullAction = false;
            }
        }

        public MessageDirection Direction
        {
            get
            {
                return this.direction;
            }
        }

        public string Action
        {
            get
            {
                return !this.isNullAction ? this.action : (string)null;
            }
        }

        public override bool Equals(object other)
        {
            return other is DirectionalAction other1 && this.Equals(other1);
        }

        public bool Equals(DirectionalAction other)
        {
            return other != null && this.direction == other.direction && this.action == other.action;
        }

        public int CompareTo(DirectionalAction other)
        {
            if (other == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(other));
            if (this.direction == MessageDirection.Input && other.direction == MessageDirection.Output)
                return -1;
            return this.direction == MessageDirection.Output && other.direction == MessageDirection.Input ? 1 : this.action.CompareTo(other.action);
        }

        public override int GetHashCode()
        {
            return this.action.GetHashCode();
        }
    }
}
