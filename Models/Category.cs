using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Expense_Tracker.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        public int? CategoryLimit { get; set; }

        [Column(TypeName = "nvarchar(5)")]
        public string Icon { get; set; } = "";

        [Column(TypeName = "nvarchar(10)")]
        public string Type { get; set; } = "Expense";

        [NotMapped]
        public string? TitleWithIcon
        {
            get
            {
                return this.Icon + " " + this.Title;
            }
        }
        [NotMapped]
        public string? Titleiconlimit
        {
            get
            {
                return this.Icon + " " + this.Title + "  " + this.CategoryLimit;
            }
        }
        [NotMapped]
        public string? FormattedCategoryLimit
        {
            get
            {
                CultureInfo cultureformatted = CultureInfo.CreateSpecificCulture("en-IND");
                cultureformatted.NumberFormat.CurrencyNegativePattern = 1;
                return String.Format(cultureformatted, "{0:C0}", CategoryLimit);
            }
        }
    }


}
