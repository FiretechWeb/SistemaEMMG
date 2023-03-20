-- MySqlBackup.NET 2.3.8.0
-- Dump Time: 2023-03-20 12:17:50
-- --------------------------------------
-- Server version 5.7.11 MySQL Community Server (GPL)


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES latin1 */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- 
-- Definition of bancos
-- 

DROP TABLE IF EXISTS `bancos`;
CREATE TABLE IF NOT EXISTS `bancos` (
  `bc_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `bc_nombre` varchar(64) NOT NULL,
  PRIMARY KEY (`bc_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table bancos
-- 

/*!40000 ALTER TABLE `bancos` DISABLE KEYS */;

/*!40000 ALTER TABLE `bancos` ENABLE KEYS */;

-- 
-- Definition of comprobantes
-- 

DROP TABLE IF EXISTS `comprobantes`;
CREATE TABLE IF NOT EXISTS `comprobantes` (
  `cm_em_id` bigint(20) NOT NULL,
  `cm_ec_id` bigint(20) NOT NULL,
  `cm_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `cm_tc_id` bigint(20) NOT NULL,
  `cm_fecha` date DEFAULT NULL,
  `cm_numero` varchar(50) NOT NULL,
  `cm_gravado` double DEFAULT NULL,
  `cm_iva` double DEFAULT NULL,
  `cm_no_gravado` double DEFAULT NULL,
  `cm_percepcion` double DEFAULT NULL,
  `cm_emitido` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`cm_em_id`,`cm_ec_id`,`cm_id`),
  KEY `fk_tipo` (`cm_tc_id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table comprobantes
-- 

/*!40000 ALTER TABLE `comprobantes` DISABLE KEYS */;

/*!40000 ALTER TABLE `comprobantes` ENABLE KEYS */;

-- 
-- Definition of comprobantes_pagos
-- 

DROP TABLE IF EXISTS `comprobantes_pagos`;
CREATE TABLE IF NOT EXISTS `comprobantes_pagos` (
  `cp_em_id` bigint(20) NOT NULL,
  `cp_ec_id` bigint(20) NOT NULL,
  `cp_cm_id` bigint(20) NOT NULL,
  `cp_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `cp_fp_id` bigint(20) NOT NULL,
  `cp_importe` double NOT NULL,
  `cp_obs` varchar(128) DEFAULT NULL,
  `cp_fecha` datetime DEFAULT NULL,
  PRIMARY KEY (`cp_em_id`,`cp_ec_id`,`cp_cm_id`,`cp_id`),
  KEY `fk_formapago` (`cp_fp_id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table comprobantes_pagos
-- 

/*!40000 ALTER TABLE `comprobantes_pagos` DISABLE KEYS */;

/*!40000 ALTER TABLE `comprobantes_pagos` ENABLE KEYS */;

-- 
-- Definition of cuentas
-- 

DROP TABLE IF EXISTS `cuentas`;
CREATE TABLE IF NOT EXISTS `cuentas` (
  `em_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `em_cuit` bigint(20) DEFAULT NULL,
  `em_rs` varchar(64) DEFAULT NULL,
  PRIMARY KEY (`em_id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table cuentas
-- 

/*!40000 ALTER TABLE `cuentas` DISABLE KEYS */;

/*!40000 ALTER TABLE `cuentas` ENABLE KEYS */;

-- 
-- Definition of cuentas_bancos
-- 

DROP TABLE IF EXISTS `cuentas_bancos`;
CREATE TABLE IF NOT EXISTS `cuentas_bancos` (
  `cb_em_id` bigint(20) NOT NULL,
  `cb_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `cb_bc_id` bigint(20) NOT NULL,
  `cb_nro` varchar(32) DEFAULT NULL,
  `cb_cbu` varchar(32) DEFAULT NULL,
  `cb_cuit` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`cb_em_id`,`cb_id`),
  KEY `fk_banco` (`cb_bc_id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table cuentas_bancos
-- 

/*!40000 ALTER TABLE `cuentas_bancos` DISABLE KEYS */;

/*!40000 ALTER TABLE `cuentas_bancos` ENABLE KEYS */;

-- 
-- Definition of ent_comerciales
-- 

DROP TABLE IF EXISTS `ent_comerciales`;
CREATE TABLE IF NOT EXISTS `ent_comerciales` (
  `ec_em_id` bigint(20) NOT NULL,
  `ec_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ec_te_id` bigint(20) NOT NULL,
  `ec_cuit` bigint(20) DEFAULT NULL,
  `ec_rs` varchar(64) DEFAULT NULL,
  `ec_email` varchar(64) DEFAULT NULL,
  `ec_telefono` varchar(50) DEFAULT NULL,
  `ec_celular` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`ec_em_id`,`ec_id`),
  KEY `fk_tipos_entidad` (`ec_te_id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table ent_comerciales
-- 

/*!40000 ALTER TABLE `ent_comerciales` DISABLE KEYS */;

/*!40000 ALTER TABLE `ent_comerciales` ENABLE KEYS */;

-- 
-- Definition of entidades_bancos
-- 

DROP TABLE IF EXISTS `entidades_bancos`;
CREATE TABLE IF NOT EXISTS `entidades_bancos` (
  `eb_em_id` bigint(20) NOT NULL,
  `eb_ec_id` bigint(20) NOT NULL,
  `eb_cb_id` bigint(20) NOT NULL,
  PRIMARY KEY (`eb_em_id`,`eb_ec_id`,`eb_cb_id`),
  CONSTRAINT `entidades_bancos_ibfk_1` FOREIGN KEY (`eb_em_id`) REFERENCES `cuentas` (`em_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table entidades_bancos
-- 

/*!40000 ALTER TABLE `entidades_bancos` DISABLE KEYS */;

/*!40000 ALTER TABLE `entidades_bancos` ENABLE KEYS */;

-- 
-- Definition of formas_pago
-- 

DROP TABLE IF EXISTS `formas_pago`;
CREATE TABLE IF NOT EXISTS `formas_pago` (
  `fp_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `fp_nombre` varchar(50) NOT NULL,
  PRIMARY KEY (`fp_id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table formas_pago
-- 

/*!40000 ALTER TABLE `formas_pago` DISABLE KEYS */;

/*!40000 ALTER TABLE `formas_pago` ENABLE KEYS */;

-- 
-- Definition of recibos
-- 

DROP TABLE IF EXISTS `recibos`;
CREATE TABLE IF NOT EXISTS `recibos` (
  `rm_em_id` bigint(20) NOT NULL,
  `rm_ec_id` bigint(20) NOT NULL,
  `rm_cm_id` bigint(20) NOT NULL,
  `rm_cp_id` bigint(20) NOT NULL,
  `rm_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `rm_tr_id` bigint(20) DEFAULT NULL,
  `rm_numero` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`rm_em_id`,`rm_ec_id`,`rm_cm_id`,`rm_cp_id`,`rm_id`),
  KEY `fk_tr_id` (`rm_tr_id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table recibos
-- 

/*!40000 ALTER TABLE `recibos` DISABLE KEYS */;

/*!40000 ALTER TABLE `recibos` ENABLE KEYS */;

-- 
-- Definition of remitos
-- 

DROP TABLE IF EXISTS `remitos`;
CREATE TABLE IF NOT EXISTS `remitos` (
  `rt_em_id` bigint(20) NOT NULL,
  `rt_ec_id` bigint(20) NOT NULL,
  `rt_cm_id` bigint(20) NOT NULL,
  `rt_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `rt_ts_id` bigint(20) NOT NULL,
  `rt_numero` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`rt_em_id`,`rt_ec_id`,`rt_cm_id`,`rt_id`),
  KEY `fk_ts_id` (`rt_ts_id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table remitos
-- 

/*!40000 ALTER TABLE `remitos` DISABLE KEYS */;

/*!40000 ALTER TABLE `remitos` ENABLE KEYS */;

-- 
-- Definition of tipos_comprobantes
-- 

DROP TABLE IF EXISTS `tipos_comprobantes`;
CREATE TABLE IF NOT EXISTS `tipos_comprobantes` (
  `tc_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `tc_nombre` varchar(50) NOT NULL,
  PRIMARY KEY (`tc_id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table tipos_comprobantes
-- 

/*!40000 ALTER TABLE `tipos_comprobantes` DISABLE KEYS */;

/*!40000 ALTER TABLE `tipos_comprobantes` ENABLE KEYS */;

-- 
-- Definition of tipos_entidades
-- 

DROP TABLE IF EXISTS `tipos_entidades`;
CREATE TABLE IF NOT EXISTS `tipos_entidades` (
  `te_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `te_nombre` varchar(32) DEFAULT NULL,
  PRIMARY KEY (`te_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table tipos_entidades
-- 

/*!40000 ALTER TABLE `tipos_entidades` DISABLE KEYS */;

/*!40000 ALTER TABLE `tipos_entidades` ENABLE KEYS */;

-- 
-- Definition of tipos_recibos
-- 

DROP TABLE IF EXISTS `tipos_recibos`;
CREATE TABLE IF NOT EXISTS `tipos_recibos` (
  `tr_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `tr_nombre` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`tr_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table tipos_recibos
-- 

/*!40000 ALTER TABLE `tipos_recibos` DISABLE KEYS */;

/*!40000 ALTER TABLE `tipos_recibos` ENABLE KEYS */;

-- 
-- Definition of tipos_remitos
-- 

DROP TABLE IF EXISTS `tipos_remitos`;
CREATE TABLE IF NOT EXISTS `tipos_remitos` (
  `ts_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ts_nombre` varchar(50) NOT NULL,
  PRIMARY KEY (`ts_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- 
-- Dumping data for table tipos_remitos
-- 

/*!40000 ALTER TABLE `tipos_remitos` DISABLE KEYS */;

/*!40000 ALTER TABLE `tipos_remitos` ENABLE KEYS */;


/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;


-- Dump completed on 2023-03-20 12:17:50
-- Total time: 0:0:0:0:134 (d:h:m:s:ms)
