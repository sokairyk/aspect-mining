using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectMining.Core.Database;

namespace AspectMining.Core.Dataset
{
    public interface IDatasetParser
    {
       Product ParseDataset(string datasetFile);
    }
}
