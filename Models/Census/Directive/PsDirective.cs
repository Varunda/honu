
namespace watchtower.Models.Census {

    public class PsDirective {

        public int ID { get; set; }

        public int TreeID { get; set; }

        public int TierID { get; set; }

        public int ObjectiveSetID { get; set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public int ImageSetID { get; set; }

        public int ImageID { get; set; }

    }

}