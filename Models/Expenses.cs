using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Models
{
    public class Expenses
    {

        [Key]
        public int ExpensesId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        public int ExpenseLimit { get; set; }


    }
}
