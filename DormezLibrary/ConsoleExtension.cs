using Dormez.Memory;
using Dormez.Templates;

namespace Dormez.Types
{
    [StrongTemplate("Vector2")]
    public class DormezLibrary : DObject
    {
        [Member("X")]
        public DNumber x { get; set; }

        [Member("Y")]
        public DNumber y { get; set; }
    }
}
