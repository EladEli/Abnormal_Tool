using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abnormal_UI.Infra;

namespace Abnormal_UI.UI.Samr
{
    public class SamrViewModel : AttackViewModel
    {
        private List<EntityObject> GroupsList { get; set; }
        public SamrViewModel()
        {
            GroupsList = new List<EntityObject>();
        }

        public bool GenerateLearningTime()
        {
            DbClient.SetCenterProfileForReplay();
            var domainId = DbClient.GetUniqueEntity(UniqueEntityType.Domain).First().Id;
            return false;
        }
        public enum SamrQueryType
        {
            EnumerateGroups,
            EnumerateUsers,
            QueryGroup,
            QueryDisplayInformation2,
            QueryUser
        }
    }
}
