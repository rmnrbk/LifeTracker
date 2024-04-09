using System.Globalization;
using LifeTracker.Models.BaseModels;
using Microsoft.AspNetCore.Identity;

// ReSharper disable StringLiteralTypo

namespace LifeTracker.DataAccess;

public static class SeedData
{
    private static string Email = "example@example.com";
    private static string Password = "12345";
    
    public static async Task Initialize(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        var user = new IdentityUser
        {
            UserName = Email,
            Email = Email,
        };
        
        await userManager.CreateAsync(user, Password);
        var userId = ((await userManager.FindByEmailAsync(Email))!).Id;
        
        var tag1 = new Tag { Name = "Чтение" };
        var tag2 = new Tag { Name = "\"МХБД\"" };
        var tag3 = new Tag { Name = "Работа" };
        var tag4 = new Tag { Name = "IT" };
        var tag5 = new Tag { Name = "Пет-проект" };
        var tag6 = new Tag { Name = "Теория" };
        var tag7 = new Tag { Name = "\"Чистый код\"" };
        var tag8 = new Tag { Name = "\"Чистая архитектура\"" };
        var tag9 = new Tag { Name = "Быт" };
        var tag10 = new Tag { Name = "Сон" };
        var tag11 = new Tag { Name = "Пространство" };
        var tag12 = new Tag { Name = "Еда" };
        var tag13 = new Tag { Name = "Уборка" };
        var tag14 = new Tag { Name = "Уход" };
        var tag15 = new Tag { Name = "Магазин" };
        var tag16 = new Tag { Name = "Спорт" };
        var tag17 = new Tag { Name = "Зал" };
        var tag18 = new Tag { Name = "Зарядка" };
        var tag19 = new Tag { Name = "Хобби" };
        var tag20 = new Tag { Name = "Музыка" };
        var tag21 = new Tag { Name = "Синтезатор" };
        var tag22 = new Tag { Name = "Гитара" };

        tag20.Parents.Add(tag19);
        tag21.Parents.Add(tag20);
        tag22.Parents.Add(tag20);
        tag2.Parents.Add(tag1);
        tag4.Parents.Add(tag3);
        tag5.Parents.Add(tag4);
        tag6.Parents.Add(tag4);
        tag7.Parents.Add(tag6);
        tag7.Parents.Add(tag1);
        tag8.Parents.Add(tag6);
        tag8.Parents.Add(tag1);
        tag10.Parents.Add(tag9);
        tag11.Parents.Add(tag9);
        tag12.Parents.Add(tag9);
        tag13.Parents.Add(tag9);
        tag14.Parents.Add(tag9);
        tag15.Parents.Add(tag9);
        tag17.Parents.Add(tag16);
        tag18.Parents.Add(tag16);

        Tag[] tags =
        {
            tag1, tag2, tag3, tag4, tag5, tag6, tag7, tag8, tag9, tag10, tag11, tag12, tag13, tag14, tag15, tag16,
            tag17, tag18, tag19, tag20, tag21, tag22,
        };
        
        foreach (var tag in tags)
        {
            tag.OwnerID = userId;
        }

        string todayDate = DateTime.Today.ToShortDateString();
        string day1BeforeTodayDate = (DateTime.Today.Date - TimeSpan.FromDays(1)).Date.ToShortDateString();
        string days2BeforeTodayDate = (DateTime.Today.Date - TimeSpan.FromDays(2)).Date.ToShortDateString();
        string days3BeforeTodayDate = (DateTime.Today.Date - TimeSpan.FromDays(3)).Date.ToShortDateString();
        string days4BeforeTodayDate = (DateTime.Today.Date - TimeSpan.FromDays(4)).Date.ToShortDateString();

        // var dateTimeFormat = "MM/dd/yyyy HH:mm:ss";
        var dateTimeFormat = "dd.MM.yyyy HH:mm:ss";

        Activity[] activities =
        {
            new()
            {
                Tag = tag14,
                Start = DateTime.ParseExact($"{days4BeforeTodayDate} 09:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days4BeforeTodayDate} 10:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag9,
                Start = DateTime.ParseExact($"{days4BeforeTodayDate} 10:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days4BeforeTodayDate} 13:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{days4BeforeTodayDate} 13:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days4BeforeTodayDate} 16:30:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag12,
                Start = DateTime.ParseExact($"{days4BeforeTodayDate} 16:40:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days4BeforeTodayDate} 17:20:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{days4BeforeTodayDate} 17:20:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days4BeforeTodayDate} 23:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag2,
                Start = DateTime.ParseExact($"{days4BeforeTodayDate} 23:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days4BeforeTodayDate} 23:38:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{days4BeforeTodayDate} 23:40:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days4BeforeTodayDate} 23:56:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag10,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 00:08:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 09:09:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag12,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 09:09:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 09:18:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag14,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 09:19:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 09:38:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag2,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 09:47:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 10:15:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag9,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 10:15:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 13:31:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag2,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 14:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 14:10:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 17:19:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 19:06:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 20:07:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 22:01:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 22:41:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 23:16:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag2,
                Start = DateTime.ParseExact($"{days3BeforeTodayDate} 23:23:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days3BeforeTodayDate} 23:44:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag10,
                Start = DateTime.ParseExact($"{days2BeforeTodayDate} 00:35:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days2BeforeTodayDate} 09:07:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag14,
                Start = DateTime.ParseExact($"{days2BeforeTodayDate} 09:09:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days2BeforeTodayDate} 10:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag9,
                Start = DateTime.ParseExact($"{days2BeforeTodayDate} 10:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days2BeforeTodayDate} 13:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{days2BeforeTodayDate} 13:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days2BeforeTodayDate} 16:30:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag12,
                Start = DateTime.ParseExact($"{days2BeforeTodayDate} 16:40:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days2BeforeTodayDate} 17:20:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{days2BeforeTodayDate} 17:20:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days2BeforeTodayDate} 23:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag2,
                Start = DateTime.ParseExact($"{days2BeforeTodayDate} 23:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{days2BeforeTodayDate} 23:38:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag10,
                Start = DateTime.ParseExact($"{days2BeforeTodayDate} 23:30:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 08:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag12,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 09:09:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 09:18:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag14,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 09:19:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 09:38:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag2,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 09:47:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 10:15:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag9,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 10:15:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 13:31:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag2,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 14:00:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 14:10:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 17:19:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 19:06:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 20:07:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 22:01:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag5,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 22:41:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 23:16:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag2,
                Start = DateTime.ParseExact($"{day1BeforeTodayDate} 23:23:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{day1BeforeTodayDate} 23:44:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },

            new()
            {
                Tag = tag10,
                Start = DateTime.ParseExact($"{todayDate} 00:35:00", dateTimeFormat, CultureInfo.InvariantCulture),
                End = DateTime.ParseExact($"{todayDate} 09:07:00", dateTimeFormat, CultureInfo.InvariantCulture),
            },
        };

        foreach (var activity in activities)
        {
            activity.OwnerID = userId;
        }

        db.Tags.AddRange(tags);
        db.Activities.AddRange(activities);
        db.SaveChanges();
    }
}