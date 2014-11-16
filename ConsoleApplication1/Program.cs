using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterSystems.Globals;
using InterSystems.Data.CacheClient;


namespace ConsoleApplication1
{
    class Program
    {
        static Connection Connect() {
            //получаем соединение
            Connection myConn = ConnectionContext.GetConnection();
            //проверяем открыто ли соединение
            if (!myConn.IsConnected())
            {
                Console.WriteLine("Подключение к БД");
                //если соединение не открыто, то подключаемся
                myConn.Connect("User", "_SYSTEM", "ЕYS");
            }

            if (myConn.IsConnected())
            {
                Console.WriteLine("Подключение к БД выполнено успешно");
                //если подключение произошло успешно, то возвращаем открытое соединение
                return myConn;
            }
            else { return null; }
        }

        static void Disconnect(Connection myConn) {
            //если соединение открыто, то его надо закрыть и освободить ресурсы
            if (myConn.IsConnected())
                myConn.Close();
        }

        static void CreateFirstBranch(NodeReference node, Connection myConn)
        {
            //добавляем 1 индекс - ИНН держателя карт
            node.AppendSubscript("111111111111");
            //добавляем значение узла и сохраняем данные в БД - ФИО держателя карт
            node.Set("Сидоров Петр Витальевич");
            //добавляем 2 индекс - название банка
            node.AppendSubscript("Приват банк");
            //добавляем значение узла и сохраняем данные в БД - ОКПО банка
            node.Set(14360570);
            //добавляем 3 индекс - номер счета
            node.AppendSubscript(29244825509100);
            //добавляем значение узла и сохраняем данные в БД - остаток на счету
            node.Set(28741.35);
            //добавляем 4 индекс - номер карты
            node.AppendSubscript(2145632596588547);
            //добавляем значение узла и сохраняем данные в БД - SLIP-информация карты в виде массива байтов
            string slip = "Сидоров ПВ/1965/Sidorov Petr";
            byte[] bytes = System.Text.Encoding.GetEncoding(1251).GetBytes(slip);
            node.Set(bytes);
            //добавляем 5 индекс - номер транзакции по карте
            node.AppendSubscript(1);
            //создаем новый список
            ValueList myList = myConn.CreateList();
            //в него передаем через запятую значения, как в $lb: признак дебета/кредита, номер счета кредита/дебета, имя получателя/отправителя, сумма, назначение
            myList.Append(0, 26032009100100, "Сидоров Петр Витальевич", 500.26, "Перевод на счет в другом банке");
            //присваиваем указатель на список значению узла
            node.Set(myList);
            //закрываем список
            myList.Close();
            //перемещаемся на уровень с 4 индексами
            node.SetSubscriptCount(4);
            //добавляем 5 индекс - номер транзакции по карте
            node.AppendSubscript(2);
            //создаем новый список
            myList = myConn.CreateList();
            //в него передаем через запятую значения, как в $lb: признак дебета/кредита, номер счета кредита/дебета, имя получателя/отправителя, сумма, назначение
            myList.Append(0, 26118962412531, "Иванов Иван Иванович", 115.54, "Плата за доставку");
            //присваиваем указатель на список значению узла
            node.Set(myList);
            //закрываем список
            myList.Close();
            Console.WriteLine("Создана информация о счете в Приват банке");
        }

        static void CreateSecondBranch(NodeReference node, Connection myConn)
        {
            //перемещаемся на уровень с 1 индексом
            node.SetSubscriptCount(1);
            //устанавливаем значение и создаем индекс на 2 уровне - название банка и его ОКПО
            node.Set(19807750, "УкрСиббанк");
            //устанавливаем значение и создаем индекс на 3 уровне - номер счета и остаток на нем, при этом передаем название банка в качестве первого индекса после ИНН держателя карты
            node.Set(65241.24, "УкрСиббанк", 26032009100100);
            //создаем массив байтов со SLIP-информацией карты
            string slip = "СидоровП | 1965 | SidorovP";
            byte[] bytes = System.Text.Encoding.GetEncoding(1251).GetBytes(slip);
            //устанавливаем значение и создаем индекс на 4 уровне - SLIP-информация и номер карты, не забываем все предыдущие индексы
            node.Set(bytes, "УкрСиббанк", 26032009100100, 6541963285249512);            
            //создаем список с данными о платеже
            ValueList myList = myConn.CreateList();
            myList.Append(1, 29244825509100, "Сидоров Петр Витальевич", 500.26, "Перевод на счет в другом банке");
            //устанавливаем значение и создаем индекс на 5 уровне - данные о платеже и номер транзакции по карте
            node.Set(myList, "УкрСиббанк", 26032009100100, 6541963285249512, 1);
            myList.Close();
            //создаем список с данными о платеже
            myList = myConn.CreateList();
            myList.Append(0, 26008962495545, "Сидоров Петр Витальевич", 1015.10, "Перевод на счет в другом банке");
            //устанавливаем значение и создаем индекс на 5 уровне - данные о платеже и номер транзакции по карте
            //в этом случае не надо менять уровень текущего индекса, поскольку мы до сих пор находимся на уровне 1 индекса и относительно его добавляем новые значения
            node.Set(myList, "УкрСиббанк", 26032009100100, 6541963285249512, 2);
            myList.Close();
            Console.WriteLine("Создана информация о счете в УкрСиббанке");
        }

        static void CreateThirdBranch(NodeReference node, Connection myConn)
        {            
            //указываем, что будем создавать новый индекс на 2 уровне - название банка
            node.SetSubscript(2, "ПУМБ");
            //создаем и сохраняем значение этого узла - ОКПО банка
            node.Set(14282829);
            //указываем, что будем создавать новый индекс на 3 уровне - номер счета
            node.SetSubscript(3, 26008962495545);
            //создаем и сохраняем значение этого узла - остаток на счету
            node.Set(126.32);
            //указываем, что будем создавать новый индекс на 4 уровне - номер карточки
            node.SetSubscript(4, 4567098712347654);
            //создаем массив байтов со SLIP-информацией карты
            string slip = "СидоровПетр 1965 SidorovPetr";
            byte[] bytes = System.Text.Encoding.GetEncoding(1251).GetBytes(slip);
            //создаем и сохраняем значение этого узла - SLIP-информация
            node.Set(bytes);
            //указываем, что будем создавать новый индекс на 5 уровне - название банка
            node.SetSubscript(5, 1);
            //создаем список с данными о платеже
            ValueList myList = myConn.CreateList();
            myList.Append(0, 29244825509100, "Иванов Иван Иванович", 115.54, "Плата за доставку");
            //создаем и сохраняем значение этого узла - данные о платеже
            node.Set(myList);
            myList.Close();
            //указываем, что будем создавать новый индекс на  уровне - название банка
            node.SetSubscript(5, 2);
            //создаем список с данными о платеже
            myList = myConn.CreateList();
            myList.Append(1, 26032009100100, "Сидоров Петр Витальевич", 1015.54, "Перевод на счет в другом банке");
            //создаем и сохраняем значение этого узла - данные о платеже
            node.Set(myList);
            myList.Close();
            Console.WriteLine("Создана информация о счете в ПУМБе");            
        }

        static void GetData(NodeReference node)
        {
            Object value = node.GetObject();
            if (value is string)
            {
                if (node.GetSubscriptCount() == 1)
                {
                    Console.WriteLine(value.ToString());
                }
                else if (node.GetSubscriptCount() == 5) {
                    ValueList outList = node.GetList();
                    outList.ResetToFirst();
                    
                    for (int i = 0; i < outList.Length-1; i++)
                    {
                        Console.Write(outList.GetNextObject()+", ");
                    }
                    Console.WriteLine(outList.GetNextObject());
                    outList.Close();
                }
                else if (node.GetSubscriptCount() == 4)
                {
                    string tempString = Encoding.GetEncoding(1251).GetString(node.GetBytes());
                    Console.WriteLine(tempString);
                }
            }
            else if (value is double)
            {
                Console.WriteLine(value.ToString());
            }
            else if (value is int)
            {
                Console.WriteLine(value.ToString());
            }
        }

        static void ReadData(NodeReference node)
        {
            try
            {
                //опускаемся на уровень ниже
                node.AppendSubscript("");
                //находим первый индекс на этом уровне
                string subscr = node.NextSubscript();
                //проверяем существует ли он
                while (!subscr.Equals(""))
                {
                    //пока идекс существует, добавляем в дерево на клиенте значение индекса из БД
                    node.SetSubscript(node.GetSubscriptCount(), subscr);
                    //проверяем есть ли в текщем узле данные
                    if (node.HasData())
                    {
                        //выводим значение текущего индекса
                        Console.Write(" ".PadLeft(node.GetSubscriptCount() * 4, '-') + subscr+" : ");
                        GetData(node);
                    }
                    //проверяем есть ли подиндексы у этого индекса, т.е. углубляемся в дерево дальше
                    if (node.HasSubnodes())
                    {
                        //если есть, то рекурсивно вызываем эту же функцию и передаем в нее указатель на текущий узел и индекс
                        ReadData(node);
                    }
                    //получаем значение следующего индекса на этом уровне
                    subscr = node.NextSubscript();
                }
            }
            catch (GlobalsException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //если возникла ошибка, то возвращемся на уровень выше
                node.SetSubscriptCount(node.GetSubscriptCount() - 1);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Connection myConn1 = Connect();

                NodeReference nodeRef = myConn1.CreateNodeReference("CardInfo");
                nodeRef.Kill();
                CreateFirstBranch(nodeRef, myConn1);
                CreateSecondBranch(nodeRef, myConn1);
                CreateThirdBranch(nodeRef, myConn1);
                nodeRef.Close();
                nodeRef = myConn1.CreateNodeReference("CardInfo");
                Console.WriteLine("Данные из БД:");
                ReadData(nodeRef);
                nodeRef.Close();
                Disconnect(myConn1);                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadKey();
        }
    }
}
