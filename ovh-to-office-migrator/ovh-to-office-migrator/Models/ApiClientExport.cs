using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTOM.Models
{
    public class ApiClientExport : ObservableObject
    {
        private string _email;

        public string Email
        {
            get { return _email; }
            set
            {
                if (Set(() => Email, ref _email, value))
                {
                    RaisePropertyChanged(() => Email);
                }
            }
        }

        private bool _canExport;

        public bool CanExport
        {
            get { return _canExport; }
            set
            {
                if (Set(() => CanExport, ref _canExport, value))
                {
                    RaisePropertyChanged(() => CanExport);
                }
            }
        }


        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                if (Set(() => Status, ref _status, value))
                {
                    RaisePropertyChanged(() => Status);
                }
            }
        }

        private string _exportUrl;

        public string ExportUrl
        {
            get { return _exportUrl; }
            set
            {
                if (Set(() => ExportUrl, ref _exportUrl, value))
                {
                    RaisePropertyChanged(() => ExportUrl);
                }
            }
        }
    }
}
