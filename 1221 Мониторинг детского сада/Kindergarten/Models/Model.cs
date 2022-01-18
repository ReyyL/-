using System.Data;

namespace Kindergarten.Models
{
    public class Model
    {
        public int Id { get; protected set; }

        public virtual string UpdateValues {get; }

        public virtual string InsertValues { get; }

        public Model()
        {

        }

        public Model(DataRow row)
        {

        }
    }
}
