/**
 * Clave pública del sitio para Google reCAPTCHA v2.
 *
 * La Site Key es pública por diseño: Google la expone en el HTML de cualquier
 * página que muestre el widget, así que vivir en el bundle no la filtra.
 *
 * La **Secret Key** es otra cosa y NO está acá ni en ningún archivo del
 * cliente: vive en `worklingua.Server/appsettings.Local.json` y solo la usa el
 * Web Service `.asmx` cuando el backend le pide validar un token.
 */
export const RECAPTCHA_SITE_KEY = '6LcaUKEtAAAAAF1w3FY2S52gZBjz0FCzUSypH3n9';
