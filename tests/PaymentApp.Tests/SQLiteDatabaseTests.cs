using PaymentApp.Domain.Database;
using PaymentApp.Domain.Database.Entities;
using PaymentApp.Domain.Database.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentApp.Tests
{
    public class SQLiteDatabaseTests
    {

        // TODO: покрыть тестами БД используя InMemory Data Source
        // Андрей, я тест сделал исключительно чтобы самому потестить что сделал.
        // Тесты нужно будет переписать

        [Fact]
        public void CheckInsert()
        {
            var db = new SqliteDatabase("Data Source=test_bank.db");
            db.Open();

            var accRepo = new SqliteAccountRepository(db);
            var trxRepo = new SqliteTransactionRepository(db);

            var account = accRepo.Insert(new AccountEntity
            {
                Number = "1102-2012-32",
                Owner = "Тест Тест",
                Balance = 1000
            });

            var fromDb = accRepo.FindById(account.Id);

            Assert.NotNull(fromDb);
            Assert.Equal("1102-2012-32", fromDb.Number);
            Assert.Equal("Тест Тест", fromDb.Owner);
            Assert.Equal(1000, fromDb.Balance);

            account.Balance += 100;

            accRepo.Update(account);

            fromDb = accRepo.FindById(account.Id);
            Assert.Equal(1100, fromDb.Balance);

            var trx = trxRepo.Insert(new TransactionEntity {
                Timestamp = DateTime.Now,
                FromAccount = account.Owner,
                Amount = 200,
                ToAccount = "Тест"
            });




            db.Close();
        }



    }
}
