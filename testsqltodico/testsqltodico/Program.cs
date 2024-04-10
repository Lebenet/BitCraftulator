// See https://aka.ms/new-console-template for more information

using testsqltodico;
using static testsqltodico.Convert;

Console.WriteLine("Hello, World!");
CleanRecipes("recipe_data");
//PrintDictionary(TransformSqlToDictionary("cargo_data"), "cargos");
SqlToJson("recipe_data_cleaned");

