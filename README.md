# Sistema de compra-venta EMMG

## Información

Este es un sistema para llevar control de los comprobantes de una empresa, clientes, proovedores, profesionales, pagos, cheques y recibos.

## TECH STACK

* C#
* WPF
* MySQL
* [SpreadSheet Light](https://spreadsheetlight.com/)
* [PDFtoPrinter] (https://github.com/svishnevsky/PDFtoPrinter)
* [Json.NET] (https://www.newtonsoft.com/json)


## How to install

1. Install Apache in the host.
2. Import the **sistemacomprobantes.sql** in a new database with the name defined in `db_databaseName` at configuracion.json
3. Open the project solution in Visual Studio.
4. Compile the project and run it.
5. Make sure the **configuracion.json** file is where the .exe is allocated.
