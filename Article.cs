using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hw3
{
    public class Article
    {
        public Article()
        {
            this.WordCounts = new Dictionary<int, int>();
        }
        public int GroupId { get; set; }

        public Dictionary<int,int> WordCounts { get; private set; }

        public double[] WordCountVector { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.GroupId, string.Join(";", this.WordCounts.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
