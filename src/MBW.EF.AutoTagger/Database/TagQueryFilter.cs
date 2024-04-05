using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace MBW.EF.AutoTagger.Database;

public delegate bool TagQueryFilter(DbContext? context, DbCommand command);