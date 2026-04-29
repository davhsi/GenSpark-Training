--select all the employees
select * from employees;
--select employees whi are from area 'ABC'
select * from employees where area = 'ABC';

--select employees who have id > 101
select * from employees where employeeid > 101;

--select employees with id in between 101 and 105
select * from employees where employeeid between 101 and 105;


-- Give me the average skill level of every employee

-- GIve me average skill level of each skill
 
-- give me the employees sorted by name

select employeeid, avg(skilllevel) as avg_skilllevel
from employeeskills
group by employeeid;

select skill, avg(skilllevel) as avg_skilllevel
from employeeskills
group by skill;

select *
from employees
order by name;

CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name TEXT,
    details JSONB
);


INSERT INTO products (name, details) VALUES 
('Laptop', '{"brand": "Apple", "specs": {"ram": "16GB", "storage": "512GB"}}'),
('Phone', '{"brand": "Samsung", "specs": {"ram": "8GB", "storage": "128GB"}}');


INSERT INTO products (name, details) VALUES 
('Desktop', '{"brand": "Samsung", "specs": {"ram": "8GB", "storage": "128GB"},"colour":"blue"}');


select * from products;

select details from products;

select details->'brand' from products;

select * from products
where  details->>'brand' = 'Apple';


select * from products
where  details->'specs'->>'ram' = '8GB';


select * from products
where  details ? 'colour'


