using ProgettoAPL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoAPL.Views
{
    public partial class TaskPage : ContentPage
    {
        public TaskPage(int taskId)
        {
            InitializeComponent();
            var viewModel = new TaskViewModel();
            BindingContext = viewModel;
            viewModel.LoadTaskDetails(taskId);
        }
    }
}
