using System.ComponentModel.DataAnnotations;

namespace SharpMind.ViewModels.Courses;

public enum CourseSortType
{
    [Display(Name = "Назва")]
    Name,
    [Display(Name = "Ціна")]
    Price,
    [Display(Name = "Популярність")]
    Popularity
}

