namespace watchtower.Models.PSB {
    
    public class PsbPracticeContact : PsbGroupContact {

        /// <summary>
        ///     Outfit tag the contact has permission to.
        ///     aliased to <see cref="PsbGroupContact.Groups"/>
        /// </summary>
        public string Tag {
            get {
                if (this.Groups.Count >= 1) {
                    return this.Groups[0];
                }
                return "";
            }
            set {
                this.Groups.Clear();
                this.Groups.Add(value.ToLower().Trim());
            }
        }

    }
}
