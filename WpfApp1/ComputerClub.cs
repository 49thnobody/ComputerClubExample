using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace WpfApp1
{
    public class ComputerClub : INotifyPropertyChanged
    {
        public ComputerClub()
        {
            Computers = new List<Computer>()
            {
                new Computer("1", 10),
                new Computer("2", 20, Computer.ComputerState.OnMaintenance),
                new Computer("3", 25),
            };

            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(OnTimerTick);
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();

            _availableStates = new List<Computer.ComputerState>
            {
                Computer.ComputerState.Free,
                Computer.ComputerState.Occupied,
                Computer.ComputerState.OnMaintenance,
            };

            _money = 20;
            Title = $"Компьютерный клуб \"У Ёжика\" - бюджет {_money}";
            UpdateFreeComputers();
        }

        private int _money;

        private DispatcherTimer _timer;
        private void OnTimerTick(object sender, EventArgs e)
        {
            foreach (var computer in _computers)
            {
                computer.SpendOneMinute();
            }
            UpdateFreeComputers();
        }

        private string _title;
        public string Title { get => _title; set => Set(ref _title, value); }

        private List<Computer> _computers;
        public List<Computer> Computers { get => _computers; private set => Set(ref _computers, value); }

        private IEnumerable<Computer> _freeComputers;
        public IEnumerable<Computer> FreeComputers { get => _freeComputers; private set => Set(ref _freeComputers, value); }

        public List<Computer.ComputerState> _availableStates;
        public List<string> AvailableStates { get => _availableStates.ConvertAll(s => s.GetDescription()); }

        public bool ServeClient(Client clientToServe, Computer desiredComputer)
        {
            var cost = clientToServe.DesiredTime / 60 * (desiredComputer.PricePerHour);
            if (!clientToServe.SpendMoney(cost))
            {
                return false;
            }
            else
            {
                var computerInListIndex = Computers.IndexOf(desiredComputer);
                Computers[computerInListIndex].Occupy(clientToServe);
                _money += cost;
                UpdateFreeComputers();
                return true;
            }
        }

        public void ChangeState(Computer computer, Computer.ComputerState state)
        {
            computer.ChangeState(state);
            UpdateFreeComputers();
        }

        private void UpdateFreeComputers()
        {
            FreeComputers = _computers.Where(c => c.State == Computer.ComputerState.Free);
        }

        public bool RemoveComputer(Computer computer)
        {
            if (computer.State == Computer.ComputerState.Occupied) return false;
            Computers.Remove(computer);
            UpdateFreeComputers();
            return true;
        }

        public string AddComputer(Computer computer)
        {
            if (_money < 100)
                return "У клуба не хватает денег";
            if (Computers.Any(c => c.InventoryNumber == computer.InventoryNumber))
                return "Компьютер с таким инвентарным номером уже есть";
            _money -= 100;
            Title = $"Компьютерный клуб \"У Ёжика\" - бюджет {_money}";
            Computers.Add(computer);
            UpdateFreeComputers();
            return "";
        }

        public string FixComputer(Computer computer)
        {
            if (computer.State != Computer.ComputerState.OnMaintenance)
                return "Компьютер не сломан";
            if (_money < 20)
                return "У клуба не хватило денег";
            _money -= 20;
            Title = $"Компьютерный клуб \"У Ёжика\" - бюджет {_money}";
            computer.ChangeState(Computer.ComputerState.Free);
            UpdateFreeComputers();
            return "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class Computer
    {
        public Computer(string inventoryNumber, int pricePerHour, ComputerState state = default)
        {
            InventoryNumber = inventoryNumber;
            PricePerHour = pricePerHour;
            State = state;
        }

        public enum ComputerState
        {
            [Description("свободен")]
            Free = 0,
            [Description("занят")]
            Occupied = 1,
            [Description("неисправен")]
            OnMaintenance = 2
        }

        public ComputerState State { get; private set; }

        public string InventoryNumber { get; private set; }
        public int PricePerHour { get; private set; }

        private Client _client;
        private int _remainingTime;

        public bool Occupy(Client client)
        {
            if (client == null) return false;
            if (State != ComputerState.Free) return false;

            _client = client;
            _remainingTime = client.DesiredTime;
            State = ComputerState.Occupied;
            return true;
        }

        public override string ToString()
        {
            string ext = State == ComputerState.Occupied ? $"ещё {_remainingTime} минут клиентом {_client}" : "";
            return $"{InventoryNumber}: {PricePerHour} в час - {State.GetDescription()}" + ext;
        }

        public void SpendOneMinute()
        {
            if (State != ComputerState.Occupied) return;

            _remainingTime--;
            if (_remainingTime == 0)
            {
                State = ComputerState.Free;
                _client = null;
                int broke = new Random().Next(0, 20);
                if (broke == 1)
                {
                    State = ComputerState.OnMaintenance;
                }
            }
        }

        public void ChangeState(ComputerState state)
        {
            State = state;
        }
    }

    public class Client
    {
        public Client(string lastName, string name, int money, int desiredTime)
        {
            LastName = lastName;
            Name = name;
            _money = money;
            DesiredTime = desiredTime;
        }

        public string LastName { get; private set; }
        public string Name { get; private set; }
        public int DesiredTime { get; private set; }

        private int _money;

        public bool SpendMoney(int moneyToSpend)
        {
            if (_money - moneyToSpend < 0) return false;
            else
            {
                _money -= moneyToSpend;
                return true;
            }
        }

        public override string ToString()
        {
            return $"{LastName} {Name}";
        }
    }

    public static class Extensions
    {
        public static string GetDescription(this Computer.ComputerState state)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])state
            .GetType()
            .GetField(state.ToString())
            .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
