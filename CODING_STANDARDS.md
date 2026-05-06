# Coding Standards - DevCore Hospital

## 1. Naming: PascalCase for types and members

Use PascalCase for class names, interface names, method names, properties, and events.

```csharp
// correct
public class ShiftRepository { }
public void AddShift(Shift newShift) { }

// wrong
public class shiftRepository { }
public void addShift(Shift newShift) { }
```

## 2. Naming: camelCase for local variables and parameters

Use camelCase for method parameters and local variables.

```csharp
// correct
public void UpdateStatus(int staffId, string status) { }

// wrong
public void UpdateStatus(int StaffId, string Status) { }
```

## 3. Naming: interfaces must start with `I`

```csharp
// correct
public interface IShiftRepository { }

// wrong
public interface ShiftRepository { }
```

## 4. Naming: only `Id` is an accepted abbreviation

All other abbreviations are forbidden in any identifier — class names, method names, parameters, locals, SQL parameter names. Allowed: `Id`. Forbidden examples: `db`, `cmd`, `repo`, `vm`, `doc`, `meds`, `tcs`, `sut`, `stf`, `apt`.

```csharp
// correct
private readonly IShiftRepository shiftRepository;
AddParameter(command, "@DoctorId", doctorId);

// wrong
private readonly IShiftRepository shiftRepo;
AddParameter(command, "@DocId", doctorId);
```

## 5. Naming: no single-character identifiers

Single-letter names are forbidden for parameters, locals, and loop variables. Use a name that describes the value's role.

```csharp
// correct
foreach (Shift shift in allShifts) { }

// wrong
foreach (var s in allShifts) { }
```

## 6. Naming: SQL parameter names must be full words

`@`-prefixed SQL parameters must match the column name they bind in full words (using PascalCase). No abbreviations.

```csharp
// correct
"VALUES (@PatientId, @DoctorId, @StartTime, @EndTime, @Status)"

// wrong
"VALUES (@PatId, @DocId, @Start, @End, @Status)"
```

## 7. Constants: method-specific magic values belong inside the method

Declare a `const` inside the method body for values used only in that method. Class-level `private const` is reserved for values shared across methods — primarily date formats, culture codes, and domain rule thresholds.

```csharp
// correct — method-local constant
public async Task CreateAppointmentAsync(...)
{
    const int appointmentDurationMinutes = 30;
    ...
}

// correct — class-level constant for shared rule
private const double FatigueThresholdHours = 12.0;
```

Error message strings do **not** become constants; leave them as inline literals.

## 8. Constants: no magic numbers or strings in logic

Every literal that encodes a business rule (default ID, status code, threshold) must be named. Exception: `0` used in an arithmetic expression where its meaning is already clear from context is acceptable.

```csharp
// correct
private const int DefaultPatientId = 0;
private const int MinValidShiftId = 1;
if (shiftId < MinValidShiftId) { return false; }

// wrong
if (shiftId <= 0) { return false; }
```

## 9. Lambdas: replace inline lambdas with named local functions

All non-trivial lambdas must be extracted to named local functions within the same method. This keeps the LINQ chain readable and gives the predicate a name that documents intent.

```csharp
// correct
bool IsForDoctor(Appointment appointment) => appointment.DoctorId == doctorId;
return allAppointments.Where(IsForDoctor).ToList();

// wrong
return allAppointments.Where(a => a.DoctorId == doctorId).ToList();
```

## 10. Repositories: single-table CRUD only

A repository class may only query the table it owns. No JOINs, no sub-selects against other tables, no `CONCAT` or formatting logic in SQL. Cross-entity data must be assembled in the service layer.

```csharp
// correct — AppointmentRepository selects only Appointments columns
SELECT appointment_id, doctor_id, patient_id, start_time, end_time, status
FROM Appointments WHERE ...

// wrong — joining Staff inside AppointmentRepository
SELECT a.*, CONCAT(s.first_name, ' ', s.last_name) AS DoctorName
FROM Appointments a INNER JOIN Staff s ON a.doctor_id = s.staff_id
```

## 11. Repositories: named column ordinals

Use `reader.GetOrdinal("column_name")` to obtain ordinals before the read loop; never use positional magic numbers inside `reader.GetXxx(n)`.

```csharp
// correct
int staffIdOrdinal = reader.GetOrdinal("staff_id");
while (reader.Read())
{
    int staffId = reader.GetInt32(staffIdOrdinal);
}

// wrong
while (reader.Read())
{
    int staffId = reader.GetInt32(0);
}
```

## 12. Architecture: no business logic in repositories

Repositories perform CRUD. Filtering by date range, computing totals, building domain objects from multiple sources, and all conditional rules belong in the service layer.

## 13. Architecture: no UI concerns in ViewModels

ViewModels must not reference `Brush`, `Color`, `SolidColorBrush`, or any `Microsoft.UI` / `Windows.UI` type. Colour bindings belong in the view layer, implemented as `IValueConverter` classes.

```csharp
// correct — ViewModel exposes a bool
public bool CanPublish => !Violations.Any();

// wrong — ViewModel creates a Brush
public Brush PublishStatusColor =>
    CanPublish ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
```

## 14. Architecture: culture and format strings as class-level constants

Hardcoded locale strings (`"en-US"`) and date format patterns (`"dd MMM yyyy"`) must be declared as `private const` / `private static readonly` fields at the class level, not inlined in expressions.

```csharp
// correct
private const string EnglishCultureCode = "en-US";
private const string WeeklyDateFormat = "dd MMM yyyy";
private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo(EnglishCultureCode);

// wrong
var culture = CultureInfo.GetCultureInfo("en-US");
return date.ToString("dd MMM yyyy", culture);
```

## 15. Tests: three-segment naming

Every test method name must contain **exactly two underscores**, producing three segments: `MethodOrFeature_WhenCondition_ProducesResult`. The middle segment must begin with `When`, `Given`, or `On`.

```csharp
// correct
public void FinishAppointmentAsync_WhenNoActiveAppointmentsRemain_SetsDoctorStatusToAvailable()

// wrong
public void FinishAppointmentAsync_SetsDoctorStatus_ToAvailable_WhenNoActiveAppointmentsRemain()
public void FinishAppointmentAsync_SetsDoctorStatusToAvailable()
```

## 16. Tests: at most two assertions per test

Each test must verify one behaviour. Use at most two `Assert` calls. If you need more, split into separate tests.

## 17. Tests: no abbreviations in test field names

Test class fields follow the same no-abbreviation rule as production code. Common violations: `sut` → use the concrete type name (e.g. `service`), `repo` → `repository`, `vm` → `viewModel`.

```csharp
// correct
private readonly DoctorAppointmentService service;
private readonly Mock<IAppointmentRepository> appointmentRepository;

// wrong
private readonly DoctorAppointmentService sut;
private readonly Mock<IAppointmentRepository> appointmentRepo;
```

## 18. Unused code: delete it

Unused variables, unreachable branches, commented-out code blocks, and backwards-compatibility shims must not be committed. If code is no longer needed, remove it entirely.

## 19. Comments: only explain non-obvious decisions

Do not comment on what the code does — well-named identifiers already do that. Write a comment only when the **why** is non-obvious: a hidden constraint, a workaround for a specific external bug, or a subtle invariant. Never write multi-line comment blocks or docstrings unless the API is public-facing.

## 20. Dependency injection: constructor injection only

Services and repositories must receive all dependencies through the constructor. No service-locator lookups, no `new` inside production classes to create collaborators.
