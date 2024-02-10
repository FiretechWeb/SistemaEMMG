-- phpMyAdmin SQL Dump
-- version 4.5.4.1
-- http://www.phpmyadmin.net
--
-- Servidor: localhost
-- Tiempo de generación: 10-02-2024 a las 00:17:51
-- Versión del servidor: 5.7.11
-- Versión de PHP: 5.6.18

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `sistemacomprobantes`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `afip_tipos_comprobantes`
--

CREATE TABLE `afip_tipos_comprobantes` (
  `afip_tc_id` bigint(20) NOT NULL,
  `afip_id` bigint(20) NOT NULL,
  `afip_nombre` varchar(64) COLLATE utf8_spanish_ci NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COLLATE=utf8_spanish_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `bancos`
--

CREATE TABLE `bancos` (
  `bnk_id` bigint(20) NOT NULL,
  `bnk_name` varchar(128) COLLATE utf8_spanish_ci NOT NULL,
  `bnk_code` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `comprobantes`
--

CREATE TABLE `comprobantes` (
  `cm_em_id` bigint(20) NOT NULL,
  `cm_ec_id` bigint(20) NOT NULL,
  `cm_id` bigint(20) NOT NULL,
  `cm_tc_id` bigint(20) NOT NULL,
  `cm_mn_id` bigint(20) NOT NULL,
  `cm_cm_id` bigint(20) DEFAULT '-1',
  `cm_fecha` date DEFAULT NULL,
  `cm_numero` varchar(50) NOT NULL,
  `cm_gravado` double DEFAULT NULL,
  `cm_iva` double DEFAULT NULL,
  `cm_no_gravado` double DEFAULT NULL,
  `cm_percepcion` double DEFAULT NULL,
  `cm_subtotal` double DEFAULT '0',
  `cm_emitido` tinyint(1) DEFAULT NULL,
  `cm_cambio` double DEFAULT NULL,
  `cm_obs` varchar(128) DEFAULT NULL,
  `cm_extentas` double NOT NULL DEFAULT '0',
  `cm_otributos` double NOT NULL DEFAULT '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `cuentas`
--

CREATE TABLE `cuentas` (
  `em_id` bigint(20) NOT NULL,
  `em_cuit` bigint(20) DEFAULT NULL,
  `em_rs` varchar(64) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `ent_comerciales`
--

CREATE TABLE `ent_comerciales` (
  `ec_em_id` bigint(20) NOT NULL,
  `ec_id` bigint(20) NOT NULL,
  `ec_te_id` bigint(20) NOT NULL,
  `ec_cuit` bigint(20) DEFAULT NULL,
  `ec_rs` varchar(128) DEFAULT NULL,
  `ec_email` varchar(64) DEFAULT NULL,
  `ec_telefono` varchar(50) DEFAULT NULL,
  `ec_celular` varchar(50) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `formas_pago`
--

CREATE TABLE `formas_pago` (
  `fp_id` bigint(20) NOT NULL,
  `fp_nombre` varchar(50) NOT NULL,
  `fp_type` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `monedas`
--

CREATE TABLE `monedas` (
  `mn_id` bigint(20) NOT NULL,
  `mn_name` varchar(50) NOT NULL,
  `mn_extranjera` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pagos`
--

CREATE TABLE `pagos` (
  `pg_em_id` bigint(20) NOT NULL,
  `pg_ec_id` bigint(20) NOT NULL,
  `pg_rc_id` bigint(20) NOT NULL,
  `pg_id` bigint(20) NOT NULL,
  `pg_fp_id` bigint(20) NOT NULL,
  `pg_mn_id` bigint(20) NOT NULL,
  `pg_importe` double NOT NULL,
  `pg_cambio` double NOT NULL,
  `pg_fecha` datetime NOT NULL,
  `pg_obs` varchar(128) DEFAULT NULL,
  `pg_bnk_id` bigint(20) NOT NULL DEFAULT '-1',
  `pg_cheque_sucursal` int(11) DEFAULT NULL,
  `pg_cheque_num` bigint(20) NOT NULL DEFAULT '0',
  `pg_cheque_serie` varchar(32) NOT NULL DEFAULT '',
  `pg_cheque_persona` varchar(128) CHARACTER SET utf8 COLLATE utf8_spanish_ci NOT NULL DEFAULT '',
  `pg_cheque_cta` varchar(64) NOT NULL DEFAULT '',
  `pg_cheque_pay` date DEFAULT NULL,
  `pg_cheque_localidad` int(11) DEFAULT NULL,
  `pg_cheque_debito` date DEFAULT NULL,
  `pg_cheque_cuit` bigint(20) DEFAULT '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `recibos`
--

CREATE TABLE `recibos` (
  `rc_em_id` bigint(20) NOT NULL,
  `rc_ec_id` bigint(20) NOT NULL,
  `rc_id` bigint(20) NOT NULL,
  `rc_tr_id` bigint(20) NOT NULL,
  `rc_emitido` tinyint(1) NOT NULL,
  `rc_nro` varchar(50) DEFAULT NULL,
  `rc_fecha` datetime DEFAULT NULL,
  `rc_obs` varchar(128) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `recibos_comprobantes`
--

CREATE TABLE `recibos_comprobantes` (
  `rp_em_id` bigint(20) NOT NULL,
  `rp_ec_id` bigint(20) NOT NULL,
  `rp_rc_id` bigint(20) NOT NULL,
  `rp_cm_id` bigint(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `remitos`
--

CREATE TABLE `remitos` (
  `rm_em_id` bigint(20) NOT NULL,
  `rm_ec_id` bigint(20) NOT NULL,
  `rm_id` bigint(20) NOT NULL,
  `rm_ts_id` bigint(20) NOT NULL,
  `rm_emitido` tinyint(1) NOT NULL,
  `rm_fecha` datetime NOT NULL,
  `rm_nro` varchar(50) DEFAULT NULL,
  `rm_obs` varchar(128) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `remitos_comprobantes`
--

CREATE TABLE `remitos_comprobantes` (
  `rt_em_id` bigint(20) NOT NULL,
  `rt_ec_id` bigint(20) NOT NULL,
  `rt_rm_id` bigint(20) NOT NULL,
  `rt_cm_id` bigint(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tipos_comprobantes`
--

CREATE TABLE `tipos_comprobantes` (
  `tc_id` bigint(20) NOT NULL,
  `tc_nombre` varchar(50) NOT NULL,
  `tc_bitflags` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tipos_entidades`
--

CREATE TABLE `tipos_entidades` (
  `te_id` bigint(20) NOT NULL,
  `te_nombre` varchar(32) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tipos_recibos`
--

CREATE TABLE `tipos_recibos` (
  `tr_id` bigint(20) NOT NULL,
  `tr_nombre` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tipos_remitos`
--

CREATE TABLE `tipos_remitos` (
  `ts_id` bigint(20) NOT NULL,
  `ts_nombre` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `user_id` bigint(20) NOT NULL,
  `user_name` varchar(32) COLLATE utf8_spanish_ci DEFAULT NULL,
  `user_pass` varchar(256) COLLATE utf8_spanish_ci DEFAULT NULL,
  `user_admin` tinyint(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish_ci;

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `afip_tipos_comprobantes`
--
ALTER TABLE `afip_tipos_comprobantes`
  ADD PRIMARY KEY (`afip_tc_id`,`afip_id`);

--
-- Indices de la tabla `bancos`
--
ALTER TABLE `bancos`
  ADD PRIMARY KEY (`bnk_id`);

--
-- Indices de la tabla `comprobantes`
--
ALTER TABLE `comprobantes`
  ADD PRIMARY KEY (`cm_em_id`,`cm_ec_id`,`cm_id`),
  ADD KEY `fk_tipo` (`cm_tc_id`);

--
-- Indices de la tabla `cuentas`
--
ALTER TABLE `cuentas`
  ADD PRIMARY KEY (`em_id`);

--
-- Indices de la tabla `ent_comerciales`
--
ALTER TABLE `ent_comerciales`
  ADD PRIMARY KEY (`ec_em_id`,`ec_id`),
  ADD KEY `fk_tipos_entidad` (`ec_te_id`);

--
-- Indices de la tabla `formas_pago`
--
ALTER TABLE `formas_pago`
  ADD PRIMARY KEY (`fp_id`);

--
-- Indices de la tabla `monedas`
--
ALTER TABLE `monedas`
  ADD PRIMARY KEY (`mn_id`);

--
-- Indices de la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD PRIMARY KEY (`pg_em_id`,`pg_ec_id`,`pg_rc_id`,`pg_id`);

--
-- Indices de la tabla `recibos`
--
ALTER TABLE `recibos`
  ADD PRIMARY KEY (`rc_em_id`,`rc_ec_id`,`rc_id`);

--
-- Indices de la tabla `recibos_comprobantes`
--
ALTER TABLE `recibos_comprobantes`
  ADD PRIMARY KEY (`rp_em_id`,`rp_ec_id`,`rp_rc_id`,`rp_cm_id`);

--
-- Indices de la tabla `remitos`
--
ALTER TABLE `remitos`
  ADD PRIMARY KEY (`rm_em_id`,`rm_ec_id`,`rm_id`);

--
-- Indices de la tabla `remitos_comprobantes`
--
ALTER TABLE `remitos_comprobantes`
  ADD PRIMARY KEY (`rt_em_id`,`rt_ec_id`,`rt_rm_id`,`rt_cm_id`);

--
-- Indices de la tabla `tipos_comprobantes`
--
ALTER TABLE `tipos_comprobantes`
  ADD PRIMARY KEY (`tc_id`);

--
-- Indices de la tabla `tipos_entidades`
--
ALTER TABLE `tipos_entidades`
  ADD PRIMARY KEY (`te_id`);

--
-- Indices de la tabla `tipos_recibos`
--
ALTER TABLE `tipos_recibos`
  ADD PRIMARY KEY (`tr_id`);

--
-- Indices de la tabla `tipos_remitos`
--
ALTER TABLE `tipos_remitos`
  ADD PRIMARY KEY (`ts_id`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`user_id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `afip_tipos_comprobantes`
--
ALTER TABLE `afip_tipos_comprobantes`
  MODIFY `afip_id` bigint(20) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT de la tabla `bancos`
--
ALTER TABLE `bancos`
  MODIFY `bnk_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;
--
-- AUTO_INCREMENT de la tabla `comprobantes`
--
ALTER TABLE `comprobantes`
  MODIFY `cm_id` bigint(20) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT de la tabla `cuentas`
--
ALTER TABLE `cuentas`
  MODIFY `em_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;
--
-- AUTO_INCREMENT de la tabla `ent_comerciales`
--
ALTER TABLE `ent_comerciales`
  MODIFY `ec_id` bigint(20) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT de la tabla `formas_pago`
--
ALTER TABLE `formas_pago`
  MODIFY `fp_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;
--
-- AUTO_INCREMENT de la tabla `monedas`
--
ALTER TABLE `monedas`
  MODIFY `mn_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;
--
-- AUTO_INCREMENT de la tabla `pagos`
--
ALTER TABLE `pagos`
  MODIFY `pg_id` bigint(20) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT de la tabla `recibos`
--
ALTER TABLE `recibos`
  MODIFY `rc_id` bigint(20) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT de la tabla `remitos`
--
ALTER TABLE `remitos`
  MODIFY `rm_id` bigint(20) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT de la tabla `tipos_comprobantes`
--
ALTER TABLE `tipos_comprobantes`
  MODIFY `tc_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;
--
-- AUTO_INCREMENT de la tabla `tipos_entidades`
--
ALTER TABLE `tipos_entidades`
  MODIFY `te_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;
--
-- AUTO_INCREMENT de la tabla `tipos_recibos`
--
ALTER TABLE `tipos_recibos`
  MODIFY `tr_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;
--
-- AUTO_INCREMENT de la tabla `tipos_remitos`
--
ALTER TABLE `tipos_remitos`
  MODIFY `ts_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;
--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `user_id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;
--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `recibos_comprobantes`
--
ALTER TABLE `recibos_comprobantes`
  ADD CONSTRAINT `recibos_comprobantes_ibfk_1` FOREIGN KEY (`rp_em_id`) REFERENCES `cuentas` (`em_id`);

--
-- Filtros para la tabla `remitos_comprobantes`
--
ALTER TABLE `remitos_comprobantes`
  ADD CONSTRAINT `remitos_comprobantes_ibfk_1` FOREIGN KEY (`rt_em_id`) REFERENCES `cuentas` (`em_id`);

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
