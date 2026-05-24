using CoreX.Domain.Entities;
using CoreX.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CoreX.UI.Tests.TestSupport;

public static class SeedData
{
    public sealed record SeededClub(Guid Id, string Name, string City);

    // Seeds two clubs in different cities, each with one trainer, one group class,
    // one vacancy. Plus two global discounts and two information materials.
    // Returns the seeded clubs so tests can reference their IDs.
    public static async Task<List<SeededClub>> SeedDiscoveryFixtureAsync(CoreXFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Bail out if already seeded by a previous test in the same fixture lifecycle.
        if (db.Clubs.Any())
        {
            return db.Clubs
                .Select(c => new SeededClub(c.Id, c.Name, c.City))
                .ToList();
        }

        // Clubs — domain ctor requires (name, city, address, latitude?, longitude?, …optionals).
        var clubA = new Club(
            name: "Energy Kyiv",
            city: "Київ",
            address: "вул. Хрещатик, 1",
            latitude: 50.4501,
            longitude: 30.5234);

        var clubB = new Club(
            name: "Forge Lviv",
            city: "Львів",
            address: "пр. Свободи, 5",
            latitude: 49.8397,
            longitude: 24.0297);

        db.Clubs.AddRange(clubA, clubB);

        // Trainer per club — ctor: (clubId, fullName, specialization, experienceYears, bio?, email?, phone?).
        var trainerA = new Trainer(
            clubId: clubA.Id,
            fullName: "Ірина Швець",
            specialization: "Силові",
            experienceYears: 7);

        var trainerB = new Trainer(
            clubId: clubB.Id,
            fullName: "Петро Шеремет",
            specialization: "Кросфіт",
            experienceYears: 5);

        db.Trainers.AddRange(trainerA, trainerB);

        // GroupClass per club — ctor: (clubId, type, audience, startTime, durationMinutes, capacity, trainerId?, price?, description?).
        var nowUtc = DateTime.UtcNow;
        var classA = new GroupClass(
            clubId: clubA.Id,
            type: "Yoga",
            audience: GroupClassAudience.Adults,
            startTime: nowUtc.AddHours(1),
            durationMinutes: 60,
            capacity: 12,
            trainerId: trainerA.Id);

        var classB = new GroupClass(
            clubId: clubB.Id,
            type: "Crossfit Lite",
            audience: GroupClassAudience.Adults,
            startTime: nowUtc.AddHours(2),
            durationMinutes: 60,
            capacity: 10,
            trainerId: trainerB.Id);

        db.GroupClasses.AddRange(classA, classB);

        // Vacancy per club — ctor: (clubId, title, description, requirements, salary?, applicationDeadline?).
        // Description is required (min 10 chars); Requirements min 5 chars. IsActive defaults to true.
        var vacancyA = new Vacancy(
            clubId: clubA.Id,
            title: "Тренер з йоги",
            description: "Шукаємо досвідченого тренера з йоги для групових занять.",
            requirements: "сертифікат");

        var vacancyB = new Vacancy(
            clubId: clubB.Id,
            title: "Адміністратор",
            description: "Адміністратор на ресепшн, робота зі змінами.",
            requirements: "досвід роботи з клієнтами");

        db.Vacancies.AddRange(vacancyA, vacancyB);

        // Global discounts — ctor: (title, startDate, endDate, description?, discountPercent?, conditions?, promoCode?).
        // IsActive defaults to true inside the ctor.
        var discountStart = nowUtc.AddDays(-1);
        var discountEnd = nowUtc.AddDays(30);

        var discountA = new Discount(
            title: "Студентам -15%",
            startDate: discountStart,
            endDate: discountEnd,
            discountPercent: 15m);

        var discountB = new Discount(
            title: "Літня акція",
            startDate: discountStart,
            endDate: discountEnd,
            discountPercent: 25m);

        db.Discounts.AddRange(discountA, discountB);

        // Information materials — ctor: (title, body, category?).
        var materialA = new InformationMaterial(
            title: "Правила відвідування",
            body: "Будь ласка, дотримуйтесь розкладу занять і поважайте інших відвідувачів клубу.");

        var materialB = new InformationMaterial(
            title: "Як забронювати тренера",
            body: "Оберіть тренера у списку, перевірте розклад і підтвердіть бронювання у своєму кабінеті.");

        db.InformationMaterials.AddRange(materialA, materialB);

        await db.SaveChangesAsync();

        return new()
        {
            new SeededClub(clubA.Id, clubA.Name, clubA.City),
            new SeededClub(clubB.Id, clubB.Name, clubB.City),
        };
    }
}
