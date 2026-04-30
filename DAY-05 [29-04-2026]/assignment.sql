-- DAY-5 [29-04-2026] - Assignment
-- Joins and Stored Procedures


-- 1. List all customers and the total number of orders they have placed.
-- Show only customers with more than 5 orders. Sort by total orders descending.

SELECT c.customerid , c.companyname,
COUNT(o.orderid) AS total_orders
FROM customers c
JOIN orders o
ON c.customerid = o.customerid
GROUP BY c.customerid, c.companyname
HAVING COUNT(o.orderid) > 5
ORDER BY total_orders DESC

-- 2. Retrieve the total sales amount per customer by joining customers, orders, and
-- order_details. Show only customers whose total sales exceed 10,000. Sort by total sales descending.

SELECT c.customerid, c.companyname,
SUM(od.unitprice * od.quantity * (1 - discount)) AS total_sales
FROM customers c
JOIN orders o
ON c.customerid = o.customerid
JOIN order_details od
ON od.orderid = o.orderid
GROUP BY c.customerid, c.companyname
HAVING SUM(od.unitprice * od.quantity * (1 - discount)) > 10000
ORDER BY total_sales DESC;


-- 3. Show only categories having more than 10 products. Sort by product count descending.

SELECT c.categoryid, c.categoryname, 
COUNT(p.productid) as total_products
FROM categories c
JOIN  products p
ON c.categoryid = p.categoryid
GROUP BY c.categoryid, c.categoryname
HAVING COUNT(p.productid) > 10
ORDER BY total_products DESC;

-- 4. Include only products where total quantity sold is greater than 100. Sort by quantity descending.

SELECT p.productid, p.productname,
SUM(od.quantity) AS total_quantity
FROM order_details od
JOIN products p
ON od.productid = p.productid
GROUP BY p.productid, p.productname
HAVING SUM(od.quantity) > 100
ORDER BY total_quantity DESC;

-- 5. Show only employees who handled more than 20 orders. Sort by order count descending.

SELECT o.employeeid,
CONCAT(e.firstname, ' ', e.lastname) AS name,
COUNT(o.orderid) AS total_orders
FROM orders o
JOIN employees e
ON o.employeeid =  e.employeeid
GROUP BY o.employeeid, e.firstname, e.lastname
HAVING COUNT(o.orderid) > 20
ORDER BY total_orders DESC;

-- 6. Show only categories with total sales above 50,000. Sort by total sales descending.

SELECT c.categoryid , c.categoryname,
SUM(od.unitprice * od.quantity * (1 - discount)) as total_sales
FROM orders o
JOIN order_details od
ON o.orderid = od.orderid
JOIN products p
ON p.productid = od.productid
JOIN categories c
ON c.categoryid = p.categoryid
GROUP BY c.categoryid
HAVING SUM(od.unitprice * od.quantity * (1 - discount)) > 50000
ORDER BY total_sales DESC;


-- 7. Show only suppliers who supply more than 5 products. Sort by product count descending.

SELECT s.supplierid, s.companyname,
COUNT(p.productid) AS total_products
FROM products p
JOIN suppliers s
ON p.supplierid = s.supplierid
GROUP BY s.supplierid , s.companyname
HAVING COUNT(p.productid) > 5
ORDER BY total_products DESC;

-- 8. Show only categories where the average price is above 30. Sort by average price descending.

SELECT c.categoryid, c.categoryname,
AVG(p.unitprice) AS avg_price
FROM products p
JOIN categories c
ON p.categoryid = c.categoryid
GROUP BY c.categoryid, c.categoryname
HAVING AVG(p.unitprice) > 30
ORDER BY avg_price DESC;


-- 9. Display the total revenue generated per employee (orders + order_details).
-- Show only employees generating more than 20,000 in revenue. Sort by revenue descending.

SELECT e.employeeid,
CONCAT(e.firstname, ' ', e.lastname) AS name,
SUM(od.unitprice * od.quantity * (1 - od.discount)) AS total_revenue
FROM orders o
JOIN order_details od
ON o.orderid = od.orderid
JOIN employees e
ON e.employeeid = o.employeeid
GROUP BY e.employeeid, e.firstname, e.lastname
HAVING SUM(od.unitprice * od.quantity * (1 - od.discount)) > 20000
ORDER BY total_revenue DESC;


-- 10. Retrieve the number of orders shipped to each country.
-- Show only countries with more than 10 orders. 
-- Sort by order count descending.

select * from orders;

SELECT o.shipcountry,
COUNT(o.orderid) AS total_orders
FROM orders o
GROUP BY o.shipcountry
HAVING COUNT(o.orderid) > 10
ORDER BY total_orders DESC;


-- 11. Find customers and the average order value (orders + order_details). 
-- Show only customers with average order value greater than 500.
-- Sort by average descending.

WITH order_total_details AS (
	SELECT o.customerid, o.orderid, 
	SUM(od.unitprice * od.quantity * (1 - od.discount)) AS order_total
	FROM orders o
	JOIN order_details od
	ON o.orderid = od.orderid
	GROUP BY o.orderid, o.customerid
	-- summing up all the inidvidual items in an order
)
SELECT c.customerid, c.companyname,
AVG(otd.order_total) AS avg_order_value
FROM customers c
JOIN order_total_details otd
ON c.customerid = otd.customerid
GROUP BY c.customerid, c.companyname
HAVING AVG(otd.order_total)  > 500
ORDER BY avg_order_value DESC;



-- 12. Get the top-selling products per category (by total quantity sold). 
-- Show only products with total quantity sold above 200. 
-- Sort within category by quantity descending.

SELECT c.categoryid, c.categoryname, p.productid, p.productname,
SUM(od.quantity) AS total_quantity
FROM products p
JOIN categories c
ON p.categoryid = c.categoryid
JOIN order_details od
ON p.productid = od.productid
GROUP BY c.categoryid, c.categoryname, p.productid, p.productname
HAVING SUM(od.quantity) > 200
ORDER BY c.categoryid, total_quantity DESC;


-- 13. Retrieve the total discount given per product (order_details).
-- Show only products where total discount exceeds 1,000. 
-- Sort by discount descending.

SELECT p.productid, p.productname, 
SUM(od.unitprice * od.quantity * od.discount) AS total_discount
FROM products p
JOIN order_details od
ON p.productid = od.productid
GROUP BY p.productid
HAVING SUM(od.unitprice * od.quantity * od.discount) > 1000
ORDER BY total_discount DESC;

-- 14. List employees and the number of unique customers they handled. 
-- Show only employees who handled more than 15 unique customers. 
-- Sort by count descending.

SELECT e.employeeid,
CONCAT(e.firstname, ' ', e.lastname) AS name,
COUNT(DISTINCT o.customerid) AS unique_orders
FROM employees e
JOIN orders o 
ON e.employeeid = o.employeeid
GROUP BY e.employeeid, e.firstname, e.lastname
HAVING COUNT(DISTINCT o.customerid) > 15
ORDER BY unique_orders DESC;


-- 15. Find the monthly total sales (year + month) using orders and order_details. 
-- Show only months where total sales exceed 30,000. 
-- Sort by year and month ascending.

SELECT TO_CHAR(o.orderdate, 'YYYY-MM') AS year_month,
SUM(od.unitprice * od.quantity * (1 - od.discount )) AS total_sales
FROM orders o
JOIN order_details od
ON o.orderid = od.orderid
GROUP BY TO_CHAR(o.orderdate, 'YYYY-MM')
HAVING SUM(od.unitprice * od.quantity * (1 - od.discount )) > 30000
ORDER BY year_month;

-- instead of TO_CHAR() and converting orderdate as a string, 
-- DATE_TRUNC("month", orderdate) can be used, it defaults to anything smaller than a month from the timestamp
-- TO_CHAR() is used then to format as string.
-- NOTE: Query 1 groups by formatted string, Query 2 groups by actual date (more reliable).

SELECT DATE_TRUNC('month', o.orderdate) AS year_month,
SUM(od.unitprice * od.quantity * (1 - od.discount )) AS total_sales
FROM orders o
JOIN order_details od
ON o.orderid = od.orderid
GROUP BY DATE_TRUNC('month', o.orderdate)
HAVING SUM(od.unitprice * od.quantity * (1 - od.discount )) > 30000
ORDER BY year_month;


-- Create a stored procedure to insert a new customer. 
-- Use a transaction so that if any required value is missing, the insert is rolled back.


CREATE OR REPLACE PROCEDURE proc_insert_customer(
    p_customer_id   CHAR(5),
    p_company_name  VARCHAR(40),
    p_contact_name  VARCHAR(30),
    p_contact_title VARCHAR(30) DEFAULT NULL,
    p_address       VARCHAR(60) DEFAULT NULL,
    p_city          VARCHAR(15) DEFAULT NULL,
    p_region        VARCHAR(15) DEFAULT NULL,
    p_postal_code   VARCHAR(10) DEFAULT NULL,
    p_country       VARCHAR(15) DEFAULT NULL,
    p_phone         VARCHAR(24) DEFAULT NULL,
    p_fax           VARCHAR(24) DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_customer_id IS NULL OR TRIM(p_customer_id) = '' THEN
        RAISE EXCEPTION 'CustomerID is required';
    END IF;

    IF p_company_name IS NULL OR TRIM(p_company_name) = '' THEN
        RAISE EXCEPTION 'CompanyName is required';
    END IF;

    IF p_contact_name IS NULL OR TRIM(p_contact_name) = '' THEN
        RAISE EXCEPTION 'ContactName is required';
    END IF;

    INSERT INTO customers (
        customerid, companyname, contactname,
        contacttitle, address, city,
        region, postalcode, country,
        phone, fax
    )
    VALUES (
        p_customer_id, p_company_name, p_contact_name,
        p_contact_title, p_address, p_city,
        p_region, p_postal_code, p_country,
        p_phone, p_fax
    );

END;
$$;

CALL proc_insert_customer ('AVISH', 'Hsivad Tech', 'Davish');


-- 2. Create a stored procedure to place a new order for an existing customer with one product. 
-- Insert into orders and order_details in a single transaction.


CREATE OR REPLACE PROCEDURE proc_place_new_order(
    p_customer_id   CHAR(5),
    p_employee_id   INT,
    p_product_id    INT,
    p_quantity      SMALLINT,
    p_unit_price    NUMERIC(10,2),
    p_discount      REAL DEFAULT 0
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_id     INT;
    v_stock        SMALLINT;
    v_db_price     NUMERIC(10,2);
    v_discontinued INT;
BEGIN
    -- validate customer exists
    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE customerid = p_customer_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist', p_customer_id;
    END IF;

    -- validate employee exists
    IF NOT EXISTS (
        SELECT 1 FROM employees WHERE employeeid = p_employee_id
    ) THEN
        RAISE EXCEPTION 'employee % does not exist', p_employee_id;
    END IF;

    -- validate product exists and fetch stock, price and discontinued status
    SELECT unitsinstock, unitprice, discontinued
    INTO v_stock, v_db_price, v_discontinued
    FROM products
    WHERE productid = p_product_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'product % does not exist', p_product_id;
    END IF;

    -- ensure product is not discontinued
    IF v_discontinued = 1 THEN
        RAISE EXCEPTION 'product % is discontinued', p_product_id;
    END IF;

    -- validate quantity
    IF p_quantity <= 0 THEN
        RAISE EXCEPTION 'quantity must be greater than 0';
    END IF;

    -- validate sufficient stock
    IF v_stock < p_quantity THEN
        RAISE EXCEPTION 'insufficient stock for product % (available: %, requested: %)',
        p_product_id, v_stock, p_quantity;
    END IF;

    -- validate discount range
    IF p_discount < 0 OR p_discount > 1 THEN
        RAISE EXCEPTION 'discount must be between 0 and 1';
    END IF;

    -- optionally enforce price consistency (use DB price if mismatch)
    IF p_unit_price <> v_db_price THEN
        RAISE NOTICE 'unit price overridden with current product price';
        p_unit_price := v_db_price;
    END IF;

    -- insert into orders with all columns
    INSERT INTO orders (
        customerid,
        employeeid,
        orderdate,
        requireddate,
        shippeddate,
        shipvia,
        freight,
        shipname,
        shipaddress,
        shipcity,
        shipregion,
        shippostalcode,
        shipcountry
    )
    VALUES (
        p_customer_id,
        p_employee_id,
        CURRENT_DATE,
        CURRENT_DATE + INTERVAL '7 days',
        NULL,
        1,
        0,
        'Default Ship Name',
        'Default Address',
        'Default City',
        NULL,
        '000000',
        'Default Country'
    )
    RETURNING orderid INTO v_order_id;

    -- insert into order_details with all columns
    INSERT INTO order_details (
        orderid,
        productid,
        unitprice,
        quantity,
        discount
    )
    VALUES (
        v_order_id,
        p_product_id,
        p_unit_price,
        p_quantity,
        p_discount
    );

    -- update stock after successful order placement
    UPDATE products
    SET unitsinstock = unitsinstock - p_quantity
    WHERE productid = p_product_id;

EXCEPTION
    WHEN OTHERS THEN
        -- rollback handled automatically on failure
        RAISE;
END;
$$;


CALL proc_place_new_order(
    'BERGS'::CHAR(5),
    2,
    3,
    20::SMALLINT,
    10.00::NUMERIC,
    0.1::REAL
);

-- 3. Create a stored procedure to update product stock after an order is placed.
-- If stock is not enough, rollback the transaction.

-- create a procedure to update product stock with explicit transaction control
-- create a procedure to update product stock with validation and automatic rollback on error
CREATE OR REPLACE PROCEDURE proc_update_product_stock(
    p_product_id INT,
    p_quantity   SMALLINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_stock SMALLINT;
BEGIN
    -- fetch stock with row lock to avoid race conditions
    SELECT unitsinstock
    INTO v_stock
    FROM products
    WHERE productid = p_product_id
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'product % does not exist', p_product_id;
    END IF;

    -- validate quantity
    IF p_quantity <= 0 THEN
        RAISE EXCEPTION 'quantity must be greater than 0';
    END IF;

    -- check stock availability
    IF v_stock < p_quantity THEN
        RAISE EXCEPTION 'insufficient stock for product % (available: %, requested: %)',
        p_product_id, v_stock, p_quantity;
    END IF;

    -- update stock
    UPDATE products
    SET unitsinstock = unitsinstock - p_quantity
    WHERE productid = p_product_id;

END;
$$;

CALL proc_update_product_stock(
    3::INT,
    5::SMALLINT
);

-- 4. Create a stored procedure to cancel an order. Delete records from order_details first, then from orders, using a transaction.
CREATE OR REPLACE PROCEDURE proc_cancel_order(
    p_order_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- validate order exists
    IF NOT EXISTS (
        SELECT 1 FROM orders WHERE orderid = p_order_id
    ) THEN
        RAISE EXCEPTION 'order % does not exist', p_order_id;
    END IF;

    -- delete dependent records from order_details first (to maintain FK integrity)
    DELETE FROM order_details
    WHERE orderid = p_order_id;

    -- delete the order from orders table
    DELETE FROM orders
    WHERE orderid = p_order_id;

END;
$$;

-- valid cancellation
CALL proc_cancel_order(10248);


-- 5. Create a stored procedure to transfer products from one supplier to another. If the old supplier or new supplier does not exist, rollback.

-- create a procedure to transfer all products from one supplier to another with validation
CREATE OR REPLACE PROCEDURE proc_transfer_products_supplier(
    p_old_supplier_id INT,
    p_new_supplier_id INT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_count INT;
BEGIN
    -- validate old supplier exists
    IF NOT EXISTS (
        SELECT 1 FROM suppliers WHERE supplierid = p_old_supplier_id
    ) THEN
        RAISE EXCEPTION 'old supplier % does not exist', p_old_supplier_id;
    END IF;

    -- validate new supplier exists
    IF NOT EXISTS (
        SELECT 1 FROM suppliers WHERE supplierid = p_new_supplier_id
    ) THEN
        RAISE EXCEPTION 'new supplier % does not exist', p_new_supplier_id;
    END IF;

    -- prevent same supplier transfer
    IF p_old_supplier_id = p_new_supplier_id THEN
        RAISE EXCEPTION 'both suppliers cannot be the same';
    END IF;

    -- update products to new supplier
    UPDATE products
    SET supplierid = p_new_supplier_id
    WHERE supplierid = p_old_supplier_id;

    -- check if any products were actually transferred
    GET DIAGNOSTICS v_count = ROW_COUNT;

    IF v_count = 0 THEN
        RAISE NOTICE 'no products found for supplier %', p_old_supplier_id;
    END IF;

END;
$$;

CALL proc_transfer_products_supplier(1, 2);


-- 6

-- create a procedure to update product prices in a category by a percentage
CREATE OR REPLACE PROCEDURE proc_update_category_price(
    p_category_id INT,
    p_percentage  NUMERIC
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_count INT;
BEGIN
    -- validate category exists
    IF NOT EXISTS (
        SELECT 1 FROM categories WHERE categoryid = p_category_id
    ) THEN
        RAISE EXCEPTION 'category % does not exist', p_category_id;
    END IF;

    -- validate percentage (> 0 required)
    IF p_percentage <= 0 THEN
        RAISE EXCEPTION 'percentage must be greater than 0';
    END IF;

    -- update product prices using percentage increase
    UPDATE products
    SET unitprice = unitprice + (unitprice * p_percentage / 100)
    WHERE categoryid = p_category_id;

    -- check if any rows were updated
    GET DIAGNOSTICS v_count = ROW_COUNT;

    IF v_count = 0 THEN
        RAISE NOTICE 'no products found for category %', p_category_id;
    END IF;

END;
$$;

-- increase prices by 10% for category 1
CALL proc_update_category_price(1, 10);


-- 7

-- create a procedure to add a new product under an existing category and supplier
CREATE OR REPLACE PROCEDURE proc_add_product(
    p_product_name     VARCHAR(40),
    p_supplier_id      INT,
    p_category_id      INT,
    p_quantity_per_unit VARCHAR(20),
    p_unit_price       NUMERIC(10,2),
    p_units_in_stock   SMALLINT,
    p_units_on_order   SMALLINT,
    p_reorder_level    SMALLINT,
    p_discontinued     INT DEFAULT 0
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_product_id INT;
BEGIN
    -- validate supplier exists
    IF NOT EXISTS (
        SELECT 1 FROM suppliers WHERE supplierid = p_supplier_id
    ) THEN
        RAISE EXCEPTION 'supplier % does not exist', p_supplier_id;
    END IF;

    -- validate category exists
    IF NOT EXISTS (
        SELECT 1 FROM categories WHERE categoryid = p_category_id
    ) THEN
        RAISE EXCEPTION 'category % does not exist', p_category_id;
    END IF;

    -- validate product name
    IF p_product_name IS NULL OR LENGTH(TRIM(p_product_name)) = 0 THEN
        RAISE EXCEPTION 'product name cannot be empty';
    END IF;

    -- validate price
    IF p_unit_price < 0 THEN
        RAISE EXCEPTION 'unit price cannot be negative';
    END IF;

    -- insert new product with all columns
    INSERT INTO products (
        productname,
        supplierid,
        categoryid,
        quantityperunit,
        unitprice,
        unitsinstock,
        unitsonorder,
        reorderlevel,
        discontinued
    )
    VALUES (
        p_product_name,
        p_supplier_id,
        p_category_id,
        p_quantity_per_unit,
        p_unit_price,
        p_units_in_stock,
        p_units_on_order,
        p_reorder_level,
        p_discontinued
    )
    RETURNING productid INTO v_product_id;

END;
$$;


-- 10

-- create a procedure to place an order with multiple products in a single transaction
CREATE OR REPLACE PROCEDURE proc_place_multi_product_order(
    p_customer_id CHAR(5),
    p_employee_id INT,
    p_items JSON   -- [{"product_id":1,"quantity":5}, ...]
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_id INT;
    v_item JSON;
    v_product_id INT;
    v_quantity SMALLINT;
    v_stock SMALLINT;
    v_price NUMERIC(10,2);
BEGIN
    -- validate customer exists
    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE customerid = p_customer_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist', p_customer_id;
    END IF;

    -- validate employee exists
    IF NOT EXISTS (
        SELECT 1 FROM employees WHERE employeeid = p_employee_id
    ) THEN
        RAISE EXCEPTION 'employee % does not exist', p_employee_id;
    END IF;

    -- insert order
    INSERT INTO orders (
        customerid,
        employeeid,
        orderdate,
        requireddate,
        shippeddate,
        shipvia,
        freight,
        shipname,
        shipaddress,
        shipcity,
        shipregion,
        shippostalcode,
        shipcountry
    )
    VALUES (
        p_customer_id,
        p_employee_id,
        CURRENT_DATE,
        CURRENT_DATE + INTERVAL '7 days',
        NULL,
        1,
        0,
        'Default Ship Name',
        'Default Address',
        'Default City',
        NULL,
        '000000',
        'Default Country'
    )
    RETURNING orderid INTO v_order_id;

    -- loop through each item in JSON array
    FOR v_item IN SELECT * FROM json_array_elements(p_items)
    LOOP
        v_product_id := (v_item->>'product_id')::INT;
        v_quantity   := (v_item->>'quantity')::SMALLINT;

        -- validate product and fetch stock + price
        SELECT unitsinstock, unitprice
        INTO v_stock, v_price
        FROM products
        WHERE productid = v_product_id
        FOR UPDATE;

        IF NOT FOUND THEN
            RAISE EXCEPTION 'product % does not exist', v_product_id;
        END IF;

        -- validate quantity
        IF v_quantity <= 0 THEN
            RAISE EXCEPTION 'invalid quantity for product %', v_product_id;
        END IF;

        -- check stock
        IF v_stock < v_quantity THEN
            RAISE EXCEPTION 'insufficient stock for product % (available: %, requested: %)',
            v_product_id, v_stock, v_quantity;
        END IF;

        -- insert order_details
        INSERT INTO order_details (
            orderid,
            productid,
            unitprice,
            quantity,
            discount
        )
        VALUES (
            v_order_id,
            v_product_id,
            v_price,
            v_quantity,
            0
        );

        -- update stock
        UPDATE products
        SET unitsinstock = unitsinstock - v_quantity
        WHERE productid = v_product_id;

    END LOOP;

END;
$$;

CALL proc_place_multi_product_order(
    'VINET'::CHAR(5),
    5,
    '[{"product_id":1,"quantity":2},
      {"product_id":2,"quantity":3}]'::JSON
);


-- 9

-- create a procedure to apply a discount to all order details for a given order
CREATE OR REPLACE PROCEDURE proc_apply_discount_to_order(
    p_order_id INT,
    p_discount REAL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_count INT;
BEGIN
    -- validate order exists
    IF NOT EXISTS (
        SELECT 1 FROM orders WHERE orderid = p_order_id
    ) THEN
        RAISE EXCEPTION 'order % does not exist', p_order_id;
    END IF;

    -- validate discount range (0 to 1)
    IF p_discount < 0 OR p_discount > 1 THEN
        RAISE EXCEPTION 'discount must be between 0 and 1';
    END IF;

    -- update all order_details for the given order
    UPDATE order_details
    SET discount = p_discount
    WHERE orderid = p_order_id;

    -- check if any rows were updated
    GET DIAGNOSTICS v_count = ROW_COUNT;

    IF v_count = 0 THEN
        RAISE NOTICE 'no order details found for order %', p_order_id;
    END IF;

END;
$$;

-- apply 10% discount
CALL proc_apply_discount_to_order(10248, 0.1);



--8

-- create a procedure to delete a customer only if they have no orders
CREATE OR REPLACE PROCEDURE proc_delete_customer_if_no_orders(
    p_customer_id CHAR(5)
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- validate customer exists
    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE customerid = p_customer_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist', p_customer_id;
    END IF;

    -- check if customer has any orders
    IF EXISTS (
        SELECT 1 FROM orders WHERE customerid = p_customer_id
    ) THEN
        RAISE EXCEPTION 'cannot delete customer % because orders exist', p_customer_id;
    END IF;

    -- delete customer (safe since no dependent orders)
    DELETE FROM customers
    WHERE customerid = p_customer_id;

END;
$$;

-- successful deletion (only if no orders exist)
CALL proc_delete_customer_if_no_orders('FISSA');


-- 7

CREATE OR REPLACE PROCEDURE proc_add_product(
    p_product_name     VARCHAR(40),
    p_supplier_id      INT,
    p_category_id      INT,
    p_quantity_per_unit VARCHAR(20),
    p_unit_price       NUMERIC(10,2),
    p_units_in_stock   SMALLINT,
    p_units_on_order   SMALLINT,
    p_reorder_level    SMALLINT,
    p_discontinued     INT DEFAULT 0
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_product_id INT;
BEGIN
    -- validate supplier exists
    IF NOT EXISTS (
        SELECT 1 FROM suppliers WHERE supplierid = p_supplier_id
    ) THEN
        RAISE EXCEPTION 'supplier % does not exist', p_supplier_id;
    END IF;

    -- validate category exists
    IF NOT EXISTS (
        SELECT 1 FROM categories WHERE categoryid = p_category_id
    ) THEN
        RAISE EXCEPTION 'category % does not exist', p_category_id;
    END IF;

    -- validate product name
    IF p_product_name IS NULL OR LENGTH(TRIM(p_product_name)) = 0 THEN
        RAISE EXCEPTION 'product name cannot be empty';
    END IF;

    -- validate price
    IF p_unit_price < 0 THEN
        RAISE EXCEPTION 'unit price cannot be negative';
    END IF;

    -- insert new product with all columns
    INSERT INTO products (
        productname,
        supplierid,
        categoryid,
        quantityperunit,
        unitprice,
        unitsinstock,
        unitsonorder,
        reorderlevel,
        discontinued
    )
    VALUES (
        p_product_name,
        p_supplier_id,
        p_category_id,
        p_quantity_per_unit,
        p_unit_price,
        p_units_in_stock,
        p_units_on_order,
        p_reorder_level,
        p_discontinued
    )
    RETURNING productid INTO v_product_id;

END;
$$;

-- add a new product
CALL proc_add_product(
    'New Choco Bar'::VARCHAR,
    1::INT,
    1::INT,
    '10 boxes x 20 bars'::VARCHAR,
    25.50::NUMERIC(10,2),
    100::SMALLINT,
    0::SMALLINT,
    10::SMALLINT,
    0::INT
);


