using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OTOM.Business;
using OTOM.Models;
using OVHApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OTOM.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private OvhApiClient api;
        private OvhExportToAzureBl ovhExportToAzureBl = new OvhExportToAzureBl();

        private string _applicationKey;

        public string ApplicationKey
        {
            get { return _applicationKey; }
            set
            {
                if (Set(() => ApplicationKey, ref _applicationKey, value))
                {
                    RaisePropertyChanged(() => ApplicationKey);
                    GetConsumerKeyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _applicationSecret;

        public string ApplicationSecret
        {
            get { return _applicationSecret; }
            set
            {
                if (Set(() => ApplicationSecret, ref _applicationSecret, value))
                {
                    RaisePropertyChanged(() => ApplicationSecret);
                    GetConsumerKeyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _consumerKeyError;

        public string ConsumerKeyError
        {
            get { return _consumerKeyError; }
            set
            {
                if (Set(() => ConsumerKeyError, ref _consumerKeyError, value))
                {
                    RaisePropertyChanged(() => ConsumerKeyError);
                }
            }
        }

        private string _validationUrl;

        public string ValidationUrl
        {
            get { return _validationUrl; }
            set
            {
                if (Set(() => ValidationUrl, ref _validationUrl, value))
                {
                    RaisePropertyChanged(() => ValidationUrl);
                }
            }
        }

        private bool _canDisplayValidateLink = false;

        public bool CanDisplayValidateLink
        {
            get { return _canDisplayValidateLink; }
            set
            {
                if (Set(() => CanDisplayValidateLink, ref _canDisplayValidateLink, value))
                {
                    RaisePropertyChanged(() => CanDisplayValidateLink);
                }
            }
        }

        private bool _canDisplayQuery = false;

        public bool CanDisplayQuery
        {
            get { return _canDisplayQuery; }
            set
            {
                if (Set(() => CanDisplayQuery, ref _canDisplayQuery, value))
                {
                    RaisePropertyChanged(() => CanDisplayQuery);
                }
            }
        }

        private bool _canDisplayDatatable = false;

        public bool CanDisplayDatatable
        {
            get { return _canDisplayDatatable; }
            set
            {
                if (Set(() => CanDisplayDatatable, ref _canDisplayDatatable, value))
                {
                    RaisePropertyChanged(() => CanDisplayDatatable);
                }
            }
        }

        private string _consumerKey;

        public string ConsumerKey
        {
            get { return _consumerKey; }
            set
            {
                if (Set(() => ConsumerKey, ref _consumerKey, value))
                {
                    RaisePropertyChanged(() => ConsumerKey);
                }
            }
        }

        private ObservableCollection<string> _organizations;

        public ObservableCollection<string> Organizations
        {
            get { return _organizations; }
            set { _organizations = value; }
        }


        private ObservableCollection<string> _services;

        public ObservableCollection<string> Services
        {
            get { return _services; }
            set { _services = value; }
        }

        private string _selectedService;

        public string SelectedService
        {
            get { return _selectedService; }
            set
            {
                if (Set(() => SelectedService, ref _selectedService, value))
                {
                    RaisePropertyChanged(() => SelectedService);
                    Task.Run(() => GetExchanges());
                }
            }
        }


        private ObservableCollection<ApiClientExport> _clients;

        public ObservableCollection<ApiClientExport> Clients
        {
            get { return _clients; }
            set { _clients = value; }
        }

        private string _selectedOrg;

        public string SelectedOrg
        {
            get { return _selectedOrg; }
            set
            {
                if (Set(() => SelectedOrg, ref _selectedOrg, value))
                {
                    RaisePropertyChanged(() => SelectedOrg);
                    Task.Run(() => GetServices());
                }
            }
        }

        private string _sasUrl;

        public string SasUrl
        {
            get { return _sasUrl; }
            set
            {
                if (Set(() => SasUrl, ref _sasUrl, value))
                {
                    RaisePropertyChanged(() => SasUrl);
                    StartExportCommand.RaiseCanExecuteChanged();
                }
            }
        }


        private RelayCommand _getConsumerKeyCommand;

        /// <summary>
        /// Gets the IncrementCommand.
        /// </summary>
        public RelayCommand GetConsumerKeyCommand
        {
            get
            {
                return _getConsumerKeyCommand ?? (_getConsumerKeyCommand = new RelayCommand(async () => await GetConsumerKey(), CanExecuteGetConsumerKeyCommand));
            }
        }

        private RelayCommand _startExportCommand;

        /// <summary>
        /// Gets the IncrementCommand.
        /// </summary>
        public RelayCommand StartExportCommand
        {
            get
            {
                return _startExportCommand ?? (_startExportCommand = new RelayCommand(async () => await StartExport(), CanExecuteExportCommand));
            }
        }

        private bool CanExecuteGetConsumerKeyCommand()
        {
            return (!string.IsNullOrWhiteSpace(ApplicationKey) && !string.IsNullOrWhiteSpace(ApplicationSecret));
        }

        private bool CanExecuteExportCommand()
        {
            return (!string.IsNullOrWhiteSpace(SasUrl));
        }

        private async Task GetConsumerKey()
        {
            ConsumerKeyError = null;
            CanDisplayValidateLink = false;

            try
            {
                api = new OvhApiClient(ApplicationKey, ApplicationSecret, OvhInfra.Europe);

                CredentialsResponse response = await api.RequestCredential(new[]{
                                    new AccessRule{ Method = "GET", Path = "/*"},
                                    new AccessRule{ Method = "PUT", Path = "/*"},
                                    new AccessRule{ Method = "POST", Path = "/*"},
                                    new AccessRule{ Method = "DELETE", Path = "/*"},
                                 });

                ConsumerKey = response.ConsumerKey;
                ValidationUrl = response.ValidationUrl;
                CanDisplayValidateLink = true;
                await CheckValidation();
            }
            catch (Exception e)
            {
                ConsumerKeyError = e.Message;
            }
        }

        public async Task CheckValidation()
        {
            bool isValid = false;
            api = new OvhApiClient(ApplicationKey, ApplicationSecret, OvhInfra.Europe, ConsumerKey);

            do
            {
                try
                {
                    string[] organizations = await api.GetEmailExchangeNames();
                    Organizations = new ObservableCollection<string>(organizations);
                    RaisePropertyChanged(() => Organizations);
                    isValid = true;
                }
                catch (Exception)
                {
                    isValid = false;
                }

            } while (!isValid);

            CanDisplayQuery = true;
        }

        private async Task GetServices()
        {
            string[] services = await api.GetEmailExchangeServiceNames(SelectedOrg);
            Services = new ObservableCollection<string>(services);
            RaisePropertyChanged(() => Services);
        }

        private async Task GetExchanges()
        {
            string[] names = await api.GetEmailExchangeServiceAccountNames(SelectedOrg, SelectedService);

            Clients = new ObservableCollection<ApiClientExport>(names.Select(s => new ApiClientExport()
            {
                Email = s
            }).OrderBy(a => a.Email).ToList());
            CanDisplayDatatable = true;
            RaisePropertyChanged(() => Clients);
        }

        private async Task StartExport()
        {
            foreach(ApiClientExport client in Clients.Where(a => a.CanExport).ToList())
            {
                await ovhExportToAzureBl.StartExportForUser(client, SasUrl, api, SelectedOrg, SelectedService);
            }
        }
    }
}
