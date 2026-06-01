CREATE SCHEMA IF NOT EXISTS quantic;

CREATE OR REPLACE PROCEDURE quantic.cancel_order(p_order_id bigint)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE quantic.orders
    SET status = 'CANCELLED'
    WHERE order_id = p_order_id
      AND status = 'PENDING';

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Pending order % was not found.', p_order_id;
    END IF;
END;
$$;

CREATE OR REPLACE PROCEDURE quantic.execute_order(
    p_order_id bigint,
    p_executed_price numeric(18, 6))
LANGUAGE plpgsql
AS $$
DECLARE
    v_account_id bigint;
    v_instrument_id bigint;
    v_side varchar(10);
    v_quantity numeric(18, 6);
    v_total_value numeric(18, 6);
    v_current_balance numeric(18, 4);
    v_existing_quantity numeric(18, 6);
    v_average_price numeric(18, 6);
    v_profit_loss numeric(18, 6) := 0;
BEGIN
    IF p_executed_price <= 0 THEN
        RAISE EXCEPTION 'Executed price must be greater than zero.';
    END IF;

    SELECT account_id, instrument_id, side, quantity
    INTO v_account_id, v_instrument_id, v_side, v_quantity
    FROM quantic.orders
    WHERE order_id = p_order_id
      AND status = 'PENDING'
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Pending order % was not found.', p_order_id;
    END IF;

    v_side := UPPER(v_side);
    v_total_value := v_quantity * p_executed_price;

    SELECT current_balance
    INTO v_current_balance
    FROM quantic.paper_accounts
    WHERE account_id = v_account_id
      AND is_active = true
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Active paper account % was not found.', v_account_id;
    END IF;

    SELECT quantity, average_price
    INTO v_existing_quantity, v_average_price
    FROM quantic.positions
    WHERE account_id = v_account_id
      AND instrument_id = v_instrument_id
    FOR UPDATE;

    IF v_side = 'BUY' THEN
        IF v_current_balance < v_total_value THEN
            RAISE EXCEPTION 'Insufficient paper balance for order %.', p_order_id;
        END IF;

        UPDATE quantic.paper_accounts
        SET current_balance = current_balance - v_total_value
        WHERE account_id = v_account_id;

        INSERT INTO quantic.positions (
            account_id, instrument_id, quantity, average_price, current_price, unrealized_pnl, updated_at)
        VALUES (
            v_account_id, v_instrument_id, v_quantity, p_executed_price, p_executed_price, 0, CURRENT_TIMESTAMP)
        ON CONFLICT (account_id, instrument_id)
        DO UPDATE SET
            average_price = (
                (quantic.positions.quantity * quantic.positions.average_price) +
                (EXCLUDED.quantity * EXCLUDED.average_price)
            ) / (quantic.positions.quantity + EXCLUDED.quantity),
            quantity = quantic.positions.quantity + EXCLUDED.quantity,
            current_price = EXCLUDED.current_price,
            updated_at = CURRENT_TIMESTAMP;
    ELSIF v_side = 'SELL' THEN
        IF v_existing_quantity IS NULL OR v_existing_quantity < v_quantity THEN
            RAISE EXCEPTION 'Insufficient position quantity for order %.', p_order_id;
        END IF;

        v_profit_loss := (p_executed_price - v_average_price) * v_quantity;

        UPDATE quantic.paper_accounts
        SET current_balance = current_balance + v_total_value
        WHERE account_id = v_account_id;

        UPDATE quantic.positions
        SET quantity = quantity - v_quantity,
            current_price = p_executed_price,
            unrealized_pnl = (p_executed_price - average_price) * (quantity - v_quantity),
            updated_at = CURRENT_TIMESTAMP
        WHERE account_id = v_account_id
          AND instrument_id = v_instrument_id;
    ELSE
        RAISE EXCEPTION 'Order side must be BUY or SELL.';
    END IF;

    UPDATE quantic.orders
    SET status = 'EXECUTED',
        executed_price = p_executed_price,
        executed_time = CURRENT_TIMESTAMP
    WHERE order_id = p_order_id;

    INSERT INTO quantic.trades (
        order_id, account_id, instrument_id, side, quantity, price, total_value, profit_loss, trade_time)
    VALUES (
        p_order_id, v_account_id, v_instrument_id, v_side, v_quantity, p_executed_price,
        v_total_value, v_profit_loss, CURRENT_TIMESTAMP);
END;
$$;
