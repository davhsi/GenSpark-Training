-- DAY-5 [29-04-2026]
-- JOINS

-- 1. INNER JOIN
-- returns only matching rows from both tables

-- 2. OUTER JOIN
    -- a. LEFT JOIN
    -- returns all rows from left table + matching rows from right (NULL if no match)

    -- b. RIGHT JOIN
    -- returns all rows from right table + matching rows from left (NULL if no match)

    -- c. FULL JOIN
    -- returns all rows from both tables (matched + unmatched, NULL where no match)

-- 3. CROSS JOIN
-- no join condition required
-- returns Cartesian Product (every row from table1 × every row from table2)
-- result has:
--   columns = columns of both tables
--   rows = (rows in table1 * rows in table2)

select * from customers cross join orders;


-- customers who never placed an order
select * from customers c left join orders o on c.customerid = o.customerid where orderid is null;

select companyname,contactname,orderdate from Customers left outer join Orders
on Customers.customerid = Orders.customerId

-- customers who never placed an order


-- 1. compliles once, generates exec plan once
-- 2. more secure
-- 3. encapsulates table
-- 4. avoids complexity

select * from customers where customerId not in 
(select distinct customerId from orders)

select * from customers c left join orders o on c.customerid = o.customerid where orderid is null;



--print the product name with the order number and quantity ordered

select od.orderid, p.productname,  od.quantity as total_quantity
from  products p
join order_details od
on p.productid = od.productid;

--

select productname, orderdate, quantity
from products p join order_details od
on p.productid = od.productid
join orders o 
on o.orderid = od.orderid

--print the same details but print the products that were never ordered too

select productname, orderdate, quantity
from products p left outer join order_details od
on p.productid = od.productid
left outer join orders o 
on o.orderid = od.orderid;
--or
select productname, orderdate, quantity
from order_details od join orders o
on o.orderid = od.orderid
right outer join products p
on od.productid = p.productid

select * from employees;
select employeeid,reportsto from employees;

select emp.employeeid, concat(emp.firstname,' ',emp.lastname) employee_fullname,
emp.reportsto, concat(mgr.firstname,' ',mgr.lastname) manager_name 
from employees emp left outer join employees mgr;


-- stored procedures


create procedure proc_greet()
language plpgsql
as $$
begin
   raise notice 'Hello World!';
end;
$$;


create or replace procedure proc_greet_name(cname varchar(100))
language plpgsql
as $$
begin
   Raise notice 'Hello %',cname;
end;
$$;


call proc_greet()

call proc_greet_name('Ramu')


create or replace procedure proc_Get_Emplopyee_details()
language plpgsql
as $$
begin
   CREATE TEMP TABLE tmp_emp AS
   select emp.employeeid, concat(emp.firstname,' ',emp.lastname) employee_fullname,
	emp.reportsto, concat(mgr.firstname,' ',mgr.lastname) manager_name 
	from employees emp left outer join employees mgr
	on emp.reportsto = mgr.employeeid;
end;
$$;

call proc_Get_Emplopyee_details();

select * from tmp_emp;

create or replace function get_Employee_and_Manager_Details()
returns table(employeeid int,employee_fullname text,reportsto int,manager_name text)
language plpgsql
as $$
begin
	return query 
	select emp.employeeid, concat(emp.firstname,' ',emp.lastname) employee_fullname,
	emp.reportsto, concat(mgr.firstname,' ',mgr.lastname) manager_name 
	from employees emp left outer join employees mgr
	on emp.reportsto = mgr.employeeid;
end;
$$;

drop function get_Employee_and_Manager_Details()

select * from get_Employee_and_Manager_Details()


-- print manager name and no of employees reporting to him
select * from employees limit 5;

select concat(mgr.firstname, ' ', mgr.lastname) as manager_name, count(*) as employee_count
from employees emp
join employees mgr on emp.reportsto = mgr.employeeid
group by mgr.employeeid;

-- print products and total quantity ordered

select p.productid, p.productname, sum(od.quantity) as total_quantity
from order_details od
join products p on od.productid = p.productid
group by p.productid, p.productname;

-- transactions

create table account(aacno int,balance float);
create table trans(id int primary key,fromacc int,toacc int, amount float);

insert into account values(1,1000),(2,2000),(3,500);

insert into trans values(1,1,2,0);

create or replace procedure proc_transact(fromacc int,toacc int,amount float)
language plpgsql
as $$
begin
   DECLARE
    current_balance float;
	tarnid int;
	begin
	 	select balance into current_balance from account where aacno = fromacc;
		 select max(id) into tarnid from trans;
		insert into trans values((tarnid+1),fromacc,toacc,amount);
	  	update account set balance = balance - amount where aacno = fromacc;
	  	update account set balance = balance + amount where aacno = toacc;
		if (current_balance - amount) > 500 then
	  		commit;
		else
		    rollback;
		end if;
	end;
end;
$$;

 call proc_transact(3,1,500);




